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

namespace Controller
{
    public enum ClientStatus
    {
        Client,
        Server
    }
    public partial class Controller : MarshalByRefObject, IMyService
    {
        private Battleground battleground;
        private GameConfig currentConfig;

        private bool _readyForBattle = false;
        private System.Timers.Timer timer_readyForBattle;
        public void YouTurn(Turn d)
        {
            Log.Print($"{this.Status} receive turn: {d.x}");
            if (d.x > 20)
                return;
            //if (this.Status == ClientStatus.Client)
            //    server.YouTurn(new Turn() { x = d.x + 1 });
            //else
            //    client.YouTurn(new Turn() { x = d.x + 1 });
            //return new Turn() { x = d.x + 1 };
        }
        public GameConfig GetGameConfig(IMyService client)
        {
            this.client = client;
            return this.gameConfig;
        }

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

            currentConfig = (GameConfig)gameConfig.Clone();

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
                mw.BattleInfo.EnemyShipsTable.SetCount(c.ID, c.Count);
            });
        }

        private void BattleInfo_StartButton_Click(object sender, RoutedEventArgs e)
        {
            _readyForBattle = true;

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
            if (this.server.ReadyForBattle())
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
            currentConfig = (GameConfig)gameConfig.Clone();
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

        public bool ReadyForBattle()
        {
            if (_readyForBattle)
            {
                Log.Print("We can start the game");
                this.client.YouTurn(new Turn() { x = 4 });
            }
            return _readyForBattle;
        }
    }


    public partial class Controller : MarshalByRefObject, IMyService
    {
        public ClientStatus Status = ClientStatus.Client;
        private GameConfig gameConfig { get; set; }

        private Dispatcher _dispatcher;
        private MainWindow mw;


        private IMyService server;
        private IMyService client;
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

            Status = ClientStatus.Server;

            NewGameController ngc = new NewGameController();
            gameConfig = ngc.StartNewGame();

            try
            {
                RemotingConfiguration.Configure("Controller.exe.config", false);
                RemotingServices.Marshal(this, "MyServiceUri");
                Utils.DumpAllInfoAboutRegisteredRemotingTypes();
            }
            catch { };

            //this.StartGame();
        }
        private void Mw_ConnectButton_Click(object sender, EventArgs e)
        {
            Status = ClientStatus.Client;

            Utils.DumpAllInfoAboutRegisteredRemotingTypes();
            server = Activator.GetObject(typeof(IMyService), "tcp://localhost:33000/MyServiceUri") as IMyService;
            Log.Print("myService1 created. Proxy? {0}", (RemotingServices.IsTransparentProxy(server) ? "YES" : "NO"));

            gameConfig = server.GetGameConfig(this);
            Log.Print("CLIENT N = {0}", gameConfig.N);

            Log.Print("Start Server");
            server.StartGame();
            Log.Print("Start Client");
            this.StartGame();
        }


        [STAThread]
        static void Main(string[] args)
        {
            new Controller().Start();
        }
    }
}
