using BattleshipUI;
using BattleshipUI.New_Game;
using BattleshipUI.StatusUI;
using ONX.Cmn;
using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Controller
{

    [Serializable]
    public class Controller : MarshalByRefObject, IMyService
    {
        public static Controller Instance;
        GameConfig gameConfig;
        MainWindow mw;
        Battleground battleground;
        public void Start()
        {
            Instance = this;
            mw = new MainWindow();
            mw.NewGameButton_Click += Mw_NewGameButton_Click;
            mw.ConnectButton_Click += Mw_ConnectButton_Click;


            mw.ShowDialog();
        }

        private void Mw_NewGameButton_Click(object sender, EventArgs e)
        {
            NewGameController ngc = new NewGameController();
            gameConfig = ngc.StartNewGame();

            battleground = new Battleground(gameConfig.N);

            var config = new List<ShipUiConfig>();
            gameConfig.shipConfigs.ForEach(sc =>
                config.Add(new ShipUiConfig()
                {
                    ID = sc.ID,
                    Count = sc.Count,
                    Skin = sc.Count
                })
            );
            mw.BuildGround(gameConfig.N, config);

            RemotingConfiguration.Configure("Controller.exe.config", false);
            Utils.DumpAllInfoAboutRegisteredRemotingTypes();
            //Log.WaitForEnter("Press EXIT to stop MyService host...");
        }
        private void Mw_ConnectButton_Click(object sender, EventArgs e)
        {
            Utils.DumpAllInfoAboutRegisteredRemotingTypes();
            var res = Activator.GetObject(typeof(IMyService), "tcp://localhost:33000/MyServiceUri") as IMyService;
            Log.Print("myService1 created. Proxy? {0}", (RemotingServices.IsTransparentProxy(res) ? "YES" : "NO"));

            gameConfig = res.GetGameConfig();
            Log.Print("CLIENT N = {0}", gameConfig.N);

            battleground = new Battleground(gameConfig.N);

            var config = new List<ShipUiConfig>();
            gameConfig.shipConfigs.ForEach(sc =>
                config.Add(new ShipUiConfig()
                {
                    ID = sc.ID,
                    Count = sc.Count,
                    Skin = sc.Count
                })
            );
            mw.BuildGround(gameConfig.N, config);
            return;
        }


        [STAThread]
        static void Main(string[] args)
        {
            new Controller().Start();
        }

        public Turn YouTurn(Turn d)
        {
            throw new NotImplementedException();
        }

        public GameConfig GetGameConfig()
        {
            Console.WriteLine("Server N = " + Instance.gameConfig?.N);
            //Thread.Sleep(5000);
            return Instance.gameConfig;
        }
    }
}
