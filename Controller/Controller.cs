using BattleshipUI;
using BattleshipUI.New_Game;
using BattleshipUI.StatusUI;
using ONX.Cmn;
using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Timers;
using System.Threading;
using ONXCmn;

namespace Controller
{
    public enum ClientStatus
    {
        Client,
        Server
    }

    public class Player : MarshalByRefObject, IMyService
    {

        private Controller parent;
        private ClientStatus status;

        public IMyService server { get; set; }
        public IMyService client { get; set; }

        public Player(ClientStatus status, Controller parent)
        {
            this.parent = parent;
            this.status = status;

            var channel = ChannelServices.GetChannel("tcp");
            if (channel != null)
                ChannelServices.UnregisterChannel(channel);

            if (status == ClientStatus.Client)
            {
                RemotingConfiguration.Configure("client.config", false);
                Utils.DumpAllInfoAboutRegisteredRemotingTypes();
                this.server = Activator.GetObject(typeof(IMyService), "tcp://localhost:33000/MyServiceUri") as IMyService;
            }
            else
            {
                RemotingConfiguration.Configure("server.config", false);
                Utils.DumpAllInfoAboutRegisteredRemotingTypes();
            }


        }

        #region IN
        public GameConfig GetGameConfig(IMyService client)
        {
            this.client = client;
            return parent.GameConfig;
        }
        public void StartGame()
        {
            parent.StartGame();
        }

        public bool ReadyForBattle()
        {
            if (parent.ReadyForBattle)
            {
                Log.Print("We can start the game");
                parent.SwitchToBattleMode(true);

                //!!
                parent.PerformTurn(null);
            }
            return parent.ReadyForBattle;
        }
        public TurnResult PerformTurn(Turn turn)
        {
            return parent.PerformTurn(turn);
        }
        #endregion

        #region OUT
        public TurnResult OnTurnComplete(Turn turn)
        {
            if (this.status == ClientStatus.Client)
            {
                return this.server.PerformTurn(turn);
            }
            else
            {
                return this.client.PerformTurn(turn);
                //Ev?.Invoke(turn);
            }
        }
        #endregion

        #region IN OUT

        #endregion

    }

    public partial class Controller : MarshalByRefObject
    {
        public void SwitchToBattleMode(bool isOutTurnNow)
        {
            mw.Cell_Click += Mw_Cell_Click;
            IsOurTurnNow = isOutTurnNow;
            _battleground.Battle();
        }

        bool IsOurTurnNow = false;
        public TurnResult PerformTurn(Turn turn)
        {
            if (this.Status == ClientStatus.Server)
            {
                Log.Print($"Resposnse from client are OK: {turn?.Row} x {turn?.Column}");
            }
            else
            {
                Log.Print($"Resposnse from server are OK: {turn?.Row} x {turn?.Column}");
            }
            TurnResult result = TurnResult.None;

            //Perform opponent's turn
            if (turn == null)
            {
                //opponent skip turn
                result = TurnResult.None;
                IsOurTurnNow = true;
            }
            else
            {
                var point = new ONXCmn.Logic.Point(turn.Row, turn.Column);
                if (_battleground.DamagePoint(point))
                {
                    //opponent damage cell
                    if (_battleground.IsGameOver) //opponent win, we lose 
                    {
                        result = TurnResult.Win;
                        IsOurTurnNow = false;

                        Task task = Task.Factory.StartNew(() => MessageBox.Show("You lose!!"));
                    }
                    else
                    {
                        var ship = _battleground.GetShipAtPoint(point);
                        if (ship.Status == ShipStatus.Dead)
                        {
                            result = TurnResult.Kill;

                            var id = ship.ConfigID;
                            var conf = _currentConfig.shipConfigs.FirstOrDefault(c => c.ID == id);
                            conf.Count--;
                        }
                        else
                            result = TurnResult.Damage;

                        IsOurTurnNow = false;
                    }
                }
                else
                {
                    //opponent miss or cell already damaged
                    result = TurnResult.Miss;
                    IsOurTurnNow = true;
                }
                RedrawAll(Owner.Me);
            }


            return result;
        }
        public void MyTurn(int row, int column)
        {
            //Our turn
            if (!IsOurTurnNow)
                return;

            if (!_enemy_battleground.PointIsFree(new ONXCmn.Logic.Point(row, column)))
                return;


            Turn result = new Turn();
            result.Row = row;
            result.Column = column;
            IsOurTurnNow = false;

            var res = NetService.OnTurnComplete(result);
            Log.Print("RESULT = " + res);

            if (res == TurnResult.Damage || res == TurnResult.Kill)
            {
                IsOurTurnNow = true;
                _enemy_battleground.ForceDamagePoint(new ONXCmn.Logic.Point(row, column));

                if (res == TurnResult.Kill)
                {
                    _enemy_battleground.ForceKillShip(new ONXCmn.Logic.Point(row, column));

                    var ship = _enemy_battleground.GetShipAtPoint(new ONXCmn.Logic.Point(row, column));
                    var id = ship.ConfigID;
                    var conf = _currentEnemyConfig.shipConfigs.FirstOrDefault(c => c.ID == id);
                    conf.Count--;
                }
            }
            else if (res == TurnResult.Miss)
            {
                _enemy_battleground.DamagePoint(new ONXCmn.Logic.Point(row, column));
            }

            RedrawAll(Owner.Me);
            RedrawAll(Owner.Enemy);

            if (res == TurnResult.Win)
            {
                MessageBox.Show("You Win");
            }
        }
        private void Mw_Cell_Click(object sender, RoutedEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);
            int column = Grid.GetColumn((UIElement)sender);
            MyTurn(row, column);
        }
    }


    public partial class Controller : MarshalByRefObject
    {
        private Battleground _battleground;
        private Battleground _enemy_battleground;
        private GameConfig _currentConfig;
        private GameConfig _currentEnemyConfig;

        public bool ReadyForBattle = false;
        private System.Timers.Timer timer_readyForBattle;

        public void StartGame()
        {
            Log.Print("current thread {0}  isBackground {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsBackground);

            Task task = Task.Factory.StartNew(() =>
            {
                _dispatcher.Invoke(() => _StartGame());
            });
        }
        private void _StartGame()
        {
            Log.Print($"{this.Status} : StartGame");
            Log.Print("current thread {0}  isBackground {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsBackground);

            if (this.Status == ClientStatus.Server)
                this.IsOurTurnNow = true;
            else this.IsOurTurnNow = false;

            _currentConfig = (GameConfig)GameConfig.Clone();
            _currentEnemyConfig = (GameConfig)GameConfig.Clone();
            _battleground = new Battleground(_currentConfig.N, GameConfig);
            _enemy_battleground = new Battleground(_currentConfig.N, GameConfig);

            var config = new List<int>();
            _currentConfig.shipConfigs.ForEach(sc => config.Add(sc.ID));

            mw.Clear();

            mw.BattleInfo.MyShipsTable.SkinButton_Click += MyShips_SkinButton_Click;
            mw.BattlegroundGrid_KeyPress += Mw_BattlegroundGrid_KeyPress;
            mw.BattleInfo.ResetButton_Click += BattleInfo_ResetButton_Click;
            mw.BattleInfo.StartButton_Click += BattleInfo_StartButton_Click;
            mw.BattleInfo.RandomButton_Click += BattleInfo_RandomButton_Click;


            mw.SetMainGridVisibility(true);
            mw.SetConnectStatusisibility(false);

            mw.BuildGround(_currentConfig.N, Owner.Me);
            mw.BuildGround(_currentConfig.N, Owner.Enemy);

            mw.BattleInfo.MyShipsTable.Generate(config);
            mw.BattleInfo.EnemyShipsTable.Generate(config);


            _currentConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.MyShipsTable.SetCount(c.ID, c.Count);
                var skin = Helper.GetImage(c.ID);
                mw.BattleInfo.MyShipsTable.SetSkin(c.ID, skin);
            });

            _currentEnemyConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.EnemyShipsTable.SetCount(c.ID, c.Count);
                var skin = Helper.GetImage(c.ID);
                mw.BattleInfo.EnemyShipsTable.SetSkin(c.ID, skin);
            });

            //mw.BattleInfo.SetStartButtonEnabledState(true);
        }

        private void BattleInfo_RandomButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReadyForBattle)
                mw.BattleInfo.SetRandomButtonEnabledState(false);
            else
            {
                Reset();
                mw.BattleInfo.SetRandomButtonEnabledState(true);
                RandomPlacement();
                if (!_currentConfig.shipConfigs.Any(sc => sc.Count > 0))
                {
                    mw.BattleInfo.SetStartButtonEnabledState(true);
                }
            }
        }
        private void RandomPlacement()
        {
            var availablePoints = _battleground.ground.AllPoints();
            Random random = new Random();

            foreach (var conf in _currentConfig.shipConfigs)
            {
                int cnt = conf.Count;
                for (int i = 0; i < cnt; i++)
                {
                    conf.Count--;
                    mw.BattleInfo.MyShipsTable.SetCount(conf.ID, conf.Count);
                    var ship = new Ship(conf);
                    ship.Orientation = ShipOrientation.Horizontal;

                    bool IsPlaced = false;

                    var _list = availablePoints.ToList();
                    var _used = new HashSet<ONXCmn.Logic.Point>();

                    while (_used.Count < _list.Count)
                    {
                        int num = random.Next(0, _list.Count);
                        ship.Position = _list[num];
                        if (_battleground.CanPlace(ship))
                        {
                            IsPlaced = _battleground.AddShip(ship);
                            availablePoints.Remove(_list[num]);
                            break;
                        }
                    }

                    if (IsPlaced) continue;

                    _used.Clear();
                    foreach (var point in _battleground.ground.AllPoints())
                    {
                        ship.Position = point;
                        if (_battleground.CanPlace(ship))
                        {
                            IsPlaced = _battleground.AddShip(ship);
                            break;
                        }
                    }

                    if (!IsPlaced)
                        throw new ArgumentException("WTF!!");
                }
            }
            RedrawAll(Owner.Me);
        }

        private void BattleInfo_StartButton_Click(object sender, RoutedEventArgs e)
        {
            ReadyForBattle = true;
            _currentConfig = (GameConfig)GameConfig.Clone();

            if (this.Status == ClientStatus.Client)
            {
                if (timer_readyForBattle != null)
                {
                    timer_readyForBattle.Stop();
                    timer_readyForBattle.Dispose();
                }

                timer_readyForBattle = new System.Timers.Timer(1000);
                timer_readyForBattle.Elapsed += ReadyForBattle_Timer;
                timer_readyForBattle.Start();
            }
        }
        private void ReadyForBattle_Timer(object sender, ElapsedEventArgs e)
        {
            Log.Print("Try call ReadyForBattle() on server");

            timer_readyForBattle.Stop();
            if (this.NetService.server.ReadyForBattle())
            {
                timer_readyForBattle.Dispose();
                SwitchToBattleMode(false);
            }
            else
                timer_readyForBattle.Start();
        }

        private Ship _currentShipInPrepare;
        private void BattleInfo_ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
        private void Reset()
        {
            _currentShipInPrepare = null;
            _currentConfig = (GameConfig)GameConfig.Clone();
            _currentConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.MyShipsTable.SetCount(c.ID, c.Count);
            });
            _battleground.Reset();
            _enemy_battleground.Reset();

            mw.BattleInfo.SetStartButtonEnabledState(false);
            RedrawAll(Owner.Me);
            RedrawAll(Owner.Enemy);
        }

        private void Mw_BattlegroundGrid_KeyPress(object sender, KeyEventArgs e)
        {
            if (_currentShipInPrepare == null)
                return;

            var init = _currentShipInPrepare.Position;
            var initOrientation = _currentShipInPrepare.Orientation;
            var cur = _currentShipInPrepare.Position;
            switch (e.Key)
            {
                case Key.Enter:
                    if (_battleground.AddShip(_currentShipInPrepare))
                    {
                        ShipConfig config = _currentConfig.shipConfigs.FirstOrDefault(sc => sc.ID == _currentShipInPrepare.ConfigID);
                        config.Count--;

                        mw.BattleInfo.MyShipsTable.SetCount(config.ID, config.Count);
                        if (config.Count == 0)
                            mw.BattleInfo.MyShipsTable.DisableShipButton(config.ID);

                        if (!_currentConfig.shipConfigs.Any(sc => sc.Count > 0))
                        {
                            mw.BattleInfo.SetStartButtonEnabledState(true);
                        }

                        _currentShipInPrepare = null;
                        RedrawAll(Owner.Me);
                        return;
                    }
                    break;
                case Key.Left:
                    cur.Column--;
                    break;
                case Key.Up:
                    cur.Row--;
                    break;
                case Key.Right:
                    cur.Column++;
                    break;
                case Key.Down:
                    cur.Row++;
                    break;
                case Key.R:
                    if (_currentShipInPrepare.Orientation == ShipOrientation.Horizontal)
                        _currentShipInPrepare.Orientation = ShipOrientation.Vertical;
                    else
                        _currentShipInPrepare.Orientation = ShipOrientation.Horizontal;
                    break;
                default:
                    return;
            }
            RedrawAll(Owner.Me);
            _currentShipInPrepare.Position = cur;
            if (!_battleground.ground.Contains(_currentShipInPrepare.GetOwnNeededSpace()))
            {
                _currentShipInPrepare.Position = init;
                _currentShipInPrepare.Orientation = initOrientation;
            }

            RedrawMove(Owner.Me);
        }

        private void RedrawAll(Owner owner)
        {
            _dispatcher.Invoke(() =>
            {
                mw.BattleInfo.SetTurnStatus(IsOurTurnNow);

                _currentConfig.shipConfigs.ForEach(c =>
                {
                    mw.BattleInfo.MyShipsTable.SetCount(c.ID, c.Count);
                });

                _currentEnemyConfig.shipConfigs.ForEach(c =>
                {
                    mw.BattleInfo.EnemyShipsTable.SetCount(c.ID, c.Count);
                });
            });


            var ground = owner == Owner.Me ? _battleground : _enemy_battleground;
            var groundArea = ground.ground.AllPoints();

            var used = new HashSet<Ship>();
            foreach (var point in groundArea)
            {
                Color color = Colors.Gray;

                if (ground.PointIsFree(point))
                {
                    color = Colors.Gray;
                }
                else if (ground.PointIsAttackShip(point))
                {
                    var ship = ground.GetShipAtPoint(point);
                    if (ship.Status == ShipStatus.Dead)
                    {
                        if (!used.Contains(ship))
                        {
                            _dispatcher.Invoke(() =>
                            {
                                mw.SetSkin(point.Row, point.Column, ship.Length, Helper.GetImage(ship.ConfigID), owner);
                                mw.DrawLineThroughColumn(point.Row, point.Column, ship.Length, owner);
                            });
                            used.Add(ship);
                        }
                    }
                    else
                    {
                        color = Colors.Blue;

                        _dispatcher.Invoke(() =>
                        {
                            mw.SetCellColor(point.Row, point.Column, color, owner);
                            mw.DrawCross(point.Row, point.Column, owner);
                        });
                    }
                    continue;
                }
                else if (ground.PointIsShip(point))
                {
                    color = Colors.Blue;
                }
                else
                {
                    color = Colors.White;
                }

                _dispatcher.Invoke(() =>
                {
                    mw.SetCellColor(point.Row, point.Column, color, owner);
                });
            }
        }
        private void RedrawMove(Owner owner)
        {
            if (_currentShipInPrepare == null) return;
            var gorund = owner == Owner.Me ? _battleground : _enemy_battleground;
            var ownArea = _currentShipInPrepare.GetOwnNeededSpace().AllPoints();
            var totalArea = _currentShipInPrepare.GetTotalNeededSpace().AllPoints();


            foreach (var point in totalArea)
            {
                bool strictMode = false;
                if (ownArea.Contains(point))
                    strictMode = true;

                Color color;
                if (gorund.PointIsFree(point, strictMode))
                {
                    color = Colors.Green;
                    if (strictMode)
                        color = Colors.Yellow;
                }
                else
                {
                    color = Colors.Red;
                }

                _dispatcher.Invoke(() =>
                {
                    mw.SetCellColor(point.Row, point.Column, color, owner);
                });
            }
        }

        private void MyShips_SkinButton_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ShipConfig shipConfig = _currentConfig.shipConfigs.FirstOrDefault(sc => sc.ID == id);
            if (shipConfig == null)
                return;
            if (shipConfig.Count == 0)
                return;

            Ship ship = new Ship(shipConfig)
            {
                Orientation = ShipOrientation.Horizontal,
                Position = new ONXCmn.Logic.Point(0, 0)
            };
            _currentShipInPrepare = ship;
            RedrawAll(Owner.Me);
            RedrawMove(Owner.Me);
            Console.WriteLine(id);
        }
    }

    public partial class Controller : MarshalByRefObject
    {
        public ClientStatus Status = ClientStatus.Client;
        public GameConfig GameConfig { get; set; }

        private Dispatcher _dispatcher;
        private MainWindow mw;


        private Player NetService;
        public void Start()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            mw = new MainWindow();
            mw.NewGameButton_Click += Mw_NewGameButton_Click;
            mw.ConnectButton_Click += Mw_ConnectButton_Click;
            mw.ShowDialog();
        }


        private void Mw_NewGameButton_Click(object sender, EventArgs e)
        {
            Log.Print("current thread {0}  isBackground {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsBackground);

            mw.Clear();


            Status = ClientStatus.Server;

            NewGameController ngc = new NewGameController();
            GameConfig = ngc.StartNewGame(Helper.configs.Values.ToList());

            mw.SetMainGridVisibility(false);
            mw.SetConnectStatusisibility(true);
            mw.SetConnectStatus("Waiting opponent..");

            if (NetService != null)
            {
                RemotingServices.Disconnect(NetService);
            }
            NetService = new Player(this.Status, this);
            RemotingServices.Marshal(NetService, "MyServiceUri");


            // this.StartGame();
        }
        private void Mw_ConnectButton_Click(object sender, EventArgs e)
        {
            mw.Clear();
            Status = ClientStatus.Client;

            mw.SetMainGridVisibility(false);
            mw.SetConnectStatusisibility(true);
            mw.SetConnectStatus("Waiting server...");

            NetService = new Player(this.Status, this);

            Log.Print("myService1 created. Proxy? {0}", (RemotingServices.IsTransparentProxy(NetService.server) ? "YES" : "NO"));
            GameConfig = NetService.server.GetGameConfig(NetService);
            Log.Print("CLIENT N = {0}", GameConfig.N);

            Log.Print("Start Server");
            NetService.server.StartGame();
            Log.Print("Start Client");
            this.StartGame();
        }


        [STAThread]
        static void Main(string[] args)
        {
            Helper.ParseXml("SkinConfig.xml");
            new Controller().Start();
        }
    }
}
