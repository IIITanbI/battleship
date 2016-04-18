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
        public event OnEventHandler Ev;

        private Controller parent;
        private ClientStatus status;

        public IMyService server { get; set; }



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
        public GameConfig GetGameConfig()
        {
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
                parent.SwitchToBattleMode();

                //!!
                parent.YouTurn(null);
            }
            return parent.ReadyForBattle;
        }
        public void YouTurn(Turn turn)
        {
            parent.YouTurn(turn);
        }
        #endregion

        #region OUT
        public void OnTurnComplete(Turn turn)
        {
            if (this.status == ClientStatus.Client)
            {
                this.server.YouTurn(turn);
            }
            else
            {
                Ev?.Invoke(turn);
            }
        }
        #endregion

        #region IN OUT

        #endregion

    }



    public partial class Controller : MarshalByRefObject
    {
        public void SwitchToBattleMode()
        {
            battleground.Battle();
        }

        int _cnt = 0;
        public void YouTurn(Turn turn)
        {
            if (this.Status == ClientStatus.Server)
            {
                Log.Print($"Resposnse from client are OK: {turn.Row} x {turn.Column}");
            }
            Turn result = new Turn();

            //Perform opponent's turn
            if (turn == null)
            {
                //opponent skip turn
                result.PreviousTurnResult = TurnResult.None;
            }
            else
            {
                var point = new ONXCmn.Logic.Point(turn.Row, turn.Column);
                if (battleground.DamagePoint(point))
                {
                    //opponent damage cell
                    if (battleground.IsGameOver) //opponent win, we lose
                        result.PreviousTurnResult = TurnResult.Win;
                    else
                        result.PreviousTurnResult = TurnResult.Damage;
                }
                else
                {
                    //opponent miss or cell already damaged
                    result.PreviousTurnResult = TurnResult.Miss;
                }
                RedrawAll();
            }

            //Our turn
            _cnt++;
            if (_cnt > 10)
                return;

            Random rand = new Random();
            int row = rand.Next(0, battleground.N);
            int column = rand.Next(0, battleground.N);

            result.Row = row;
            result.Column = column;
            NetService.OnTurnComplete(result);
        }



    }


    public partial class Controller : MarshalByRefObject
    {
        private Battleground battleground;
        private GameConfig currentConfig;

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

            currentConfig = (GameConfig)GameConfig.Clone();

            battleground = new Battleground(currentConfig.N);

            var config = new List<int>();
            currentConfig.shipConfigs.ForEach(sc => config.Add(sc.ID));

            mw.BattleInfo.MyShipsTable.SkinButton_Click += MyShips_SkinButton_Click;
            mw.BattlegroundGrid_KeyPress += Mw_BattlegroundGrid_KeyPress;
            mw.BattleInfo.ResetButton_Click += BattleInfo_ResetButton_Click;
            mw.BattleInfo.StartButton_Click += BattleInfo_StartButton_Click;

            mw.BuildGround(currentConfig.N);

            mw.BattleInfo.MyShipsTable.Generate(config);
            mw.BattleInfo.EnemyShipsTable.Generate(config);

            currentConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.MyShipsTable.SetCount(c.ID, c.Count);
            });

            currentConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.EnemyShipsTable.SetCount(c.ID, -1);
            });

            mw.BattleInfo.SetStartButtonEnabledState(true);
        }

        private void BattleInfo_StartButton_Click(object sender, RoutedEventArgs e)
        {
            ReadyForBattle = true;

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
            else
            {
                foreach(var conf in currentConfig.shipConfigs)
                {
                    for (int i = 0; i < conf.Count; i++)
                    {
                        var ship = new Ship(conf);
                        ship.Orientation = ShipOrientation.Horizontal;

                        bool IsPlaced = false;
                        foreach (var point in battleground.ground.AllPoints())
                        {
                            ship.Position = point;
                            if (battleground.CanPlace(ship))
                            {
                                IsPlaced = battleground.AddShip(ship);
                                break;
                            }
                        }

                        if (IsPlaced) continue;

                        foreach (var point in battleground.ground.AllPoints())
                        {
                            ship.Position = point;
                            if (battleground.CanPlace(ship))
                            {
                                IsPlaced = battleground.AddShip(ship);
                                break;
                            }
                        }

                        if (!IsPlaced)
                            throw new ArgumentException("WTF!!");
                    }
                }
                RedrawAll();
                NetService.OnTurnComplete(new Turn(1, 1));
            }
        }
        private void ReadyForBattle_Timer(object sender, ElapsedEventArgs e)
        {
            Log.Print("Try call ReadyForBattle() on server");
            if (this.NetService.ReadyForBattle())
            {
                timer_readyForBattle.Stop();
                timer_readyForBattle.Dispose();
            }
        }

        private Ship _currentShipInPrepare;
        private void BattleInfo_ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Print("current thread {0}  isBackground {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsBackground);

            _currentShipInPrepare = null;
            currentConfig = (GameConfig)GameConfig.Clone();
            currentConfig.shipConfigs.ForEach(c =>
            {
                mw.BattleInfo.MyShipsTable.SetCount(c.ID, c.Count);
            });
            battleground.Objects.Clear();
            mw.BattleInfo.SetStartButtonEnabledState(false);
            RedrawAll();
        }
        private void Mw_BattlegroundGrid_KeyPress(object sender, KeyEventArgs e)
        {
            if (_currentShipInPrepare == null)
                return;

            var init = _currentShipInPrepare.Position;
            var cur = _currentShipInPrepare.Position;
            switch (e.Key)
            {
                case Key.Enter:
                    if (battleground.AddShip(_currentShipInPrepare))
                    {
                        ShipConfig config = currentConfig.shipConfigs.FirstOrDefault(sc => sc.ID == _currentShipInPrepare.ConfigID);
                        config.Count--;

                        mw.BattleInfo.MyShipsTable.SetCount(config.ID, config.Count);
                        if (config.Count == 0)
                            mw.BattleInfo.MyShipsTable.DisableShipButton(config.ID);

                        if (!currentConfig.shipConfigs.Any(sc => sc.Count > 0))
                        {
                            mw.BattleInfo.SetStartButtonEnabledState(true);
                        }

                        _currentShipInPrepare = null;
                        RedrawAll();
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
            RedrawAll();
            _currentShipInPrepare.Position = cur;

            RedrawMove();
        }

        private void RedrawAll()
        {
            var groundArea = battleground.ground.AllPoints();
            foreach (var point in groundArea)
            {
                Color color;

                if (battleground.PointIsFree(point))
                {
                    color = Colors.Gray;
                }
                else if (battleground.PointIsAttackShip(point))
                {
                    color = Colors.Yellow;
                }
                else if (battleground.PointIsShip(point))
                {
                    color = Colors.Blue;
                }
                else
                {
                    color = Colors.Black;
                }

                _dispatcher.Invoke(() =>
                {
                    mw.SetCellColor(point.Row, point.Column, color);
                });
            }
        }
        private void RedrawMove()
        {
            if (_currentShipInPrepare == null) return;
            var ownArea = _currentShipInPrepare.GetOwnNeededSpace().AllPoints();
            var totalArea = _currentShipInPrepare.GetTotalNeededSpace().AllPoints();


            foreach (var point in totalArea)
            {
                bool strictMode = false;
                if (ownArea.Contains(point))
                    strictMode = true;

                Color color;
                if (battleground.PointIsFree(point, strictMode))
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
                    mw.SetCellColor(point.Row, point.Column, color);
                });
            }
        }

        private void MyShips_SkinButton_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            ShipConfig shipConfig = currentConfig.shipConfigs.FirstOrDefault(sc => sc.ID == id);
            if (shipConfig == null)
                return;

            Ship ship = new Ship(shipConfig)
            {
                Orientation = ShipOrientation.Horizontal,
                Position = new ONXCmn.Logic.Point(0, 0)
            };
            _currentShipInPrepare = ship;
            RedrawAll();
            RedrawMove();
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
        private EventSink sink_;


        private void Mw_NewGameButton_Click(object sender, EventArgs e)
        {
            Log.Print("current thread {0}  isBackground {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsBackground);

            Status = ClientStatus.Server;

            NewGameController ngc = new NewGameController();
            GameConfig = ngc.StartNewGame();

            if (NetService != null)
            {
                RemotingServices.Disconnect(NetService);
            }
            NetService = new Player(this.Status, this);
            RemotingServices.Marshal(NetService, "MyServiceUri");

            //this.StartGame();
        }
        private void Mw_ConnectButton_Click(object sender, EventArgs e)
        {
            Status = ClientStatus.Client;
            NetService = new Player(this.Status, this);
            Log.Print("myService1 created. Proxy? {0}", (RemotingServices.IsTransparentProxy(NetService.server) ? "YES" : "NO"));

            GameConfig = NetService.server.GetGameConfig();
            Log.Print("CLIENT N = {0}", GameConfig.N);

            sink_ = new EventSink(new OnEventHandler(Server_Ev));
            sink_.Register(NetService.server);


            Log.Print("Start Server");
            NetService.server.StartGame();
            Log.Print("Start Client");
            this.StartGame();
        }

        private void Server_Ev(Turn turn)
        {
            Log.Print($"Resposnse from server are OK: {turn.Row} x {turn.Column}");
            this.YouTurn(turn);
        }

        [STAThread]
        static void Main(string[] args)
        {
            new Controller().Start();
        }
    }
}
