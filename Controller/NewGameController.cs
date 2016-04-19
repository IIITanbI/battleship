using BattleshipUI.New_Game;
using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Controller
{
    public class ShipUiConfig : ICloneable
    {
        public ShipConfig ShipConfig;
        public string SkinPath;

        public object Clone()
        {
            var tt = new ShipUiConfig();
            tt.ShipConfig = (ShipConfig)ShipConfig.Clone();
            tt.SkinPath = SkinPath;
            return tt;
        }
    }
    public class NewGameController
    {
        private NewGame newGame;

        private void NewGame_OkButton_Click(object sender, RoutedEventArgs e)
        {
            string text = newGame.ResponseTextBoxText;
            int n;

            if (!int.TryParse(text, out n) ||
               n < 10 || n > 20)
            {
                MessageBox.Show("Invalid value", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            newGame.DialogResult = true;
        }
        List<ShipUiConfig> UIConfigs = new List<ShipUiConfig>();

        public GameConfig StartNewGame(List<ShipUiConfig> uiConfigs)
        {
            GameConfig gameConfig = new GameConfig();

            //List<ShipConfig> shipConfigs = new List<ShipConfig>();
            //for (int length = 4; length <= 4; length++)
            //{
            //    int count = 5 - length;
            //    shipConfigs.Add(new ShipConfig()
            //    {
            //        ID = length,
            //        Count = count,
            //        Length = length
            //    });
            //}
            //gameConfig.N = 15;
            //gameConfig.shipConfigs = shipConfigs;
            //return gameConfig;


            this.UIConfigs = uiConfigs;
            var ids = UIConfigs.Select(x => x.ShipConfig.ID).ToList();
            newGame = new NewGame();
            newGame.TextChanged += NewGame_TextChanged;
            newGame.OkButton_Click += NewGame_OkButton_Click;

            newGame.Generate(ids);

            ids.ForEach(id =>
            {
                int count = UIConfigs.FirstOrDefault(x => x.ShipConfig.ID == id).ShipConfig.Count;
                newGame.SetCount(id, count);
            });

            if (newGame.ShowDialog() == true)
            {
                List<ShipUiConfig> currentConfig = new List<ShipUiConfig>();
                UIConfigs.ForEach(config =>
                {
                    var tt = (ShipUiConfig)config.Clone();
                    tt.ShipConfig.Count = newGame.GetCount(config.ShipConfig.ID);
                    if (tt.ShipConfig.Count > 0)
                        currentConfig.Add(tt);
                });

                newGame.Tag = currentConfig;

                string text = newGame.ResponseTextBoxText;
                int n;
                if (int.TryParse(text, out n))
                {
                    gameConfig.N = n;
                    gameConfig.shipConfigs = currentConfig.Select(c => c.ShipConfig).ToList();
                    return gameConfig;
                }
            }

            gameConfig.N = -1;
            return gameConfig;
        }

        private void NewGame_TextChanged(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int id = (int)textBox.Tag;
            ShipConfig shipConfig = this.UIConfigs.FirstOrDefault(sc => sc.ShipConfig.ID == id).ShipConfig;
            if (shipConfig == null)
                return;

            int value;
            if (int.TryParse(textBox.Text, out value))
            {
                //parse ok
                if (value >= 0 && value <= shipConfig.Count)
                {
                    //it's ok
                }
                else
                {
                    newGame.SetCount(id, shipConfig.Count);
                }

            }
            else
            {
                //all bad
                newGame.SetCount(id, shipConfig.Count);
            }


        }
    }
}
