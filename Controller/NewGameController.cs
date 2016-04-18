using BattleshipUI.New_Game;
using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Controller
{
    public class NewGameController
    {
        private NewGame newGame;

        public NewGameController()
        {
            newGame = new NewGame();
            newGame.OkButton_Click += NewGame_OkButton_Click;
        }

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

        public GameConfig StartNewGame()
        {
            GameConfig gameConfig = new GameConfig();

            List<ShipConfig> shipConfigs = new List<ShipConfig>();
            for (int length = 1; length <= 4; length++)
            {
                int count = 5 - length;
                shipConfigs.Add(new ShipConfig()
                {
                    ID = length,
                    Count = count,
                    Length = length
                });
            }
            gameConfig.N = 15;
            gameConfig.shipConfigs = shipConfigs;
            return gameConfig;

            if (newGame.ShowDialog() == true)
            {
                string text = newGame.ResponseTextBoxText;
                int n;
                if (int.TryParse(text, out n))
                {
                    gameConfig.N = n;
                    gameConfig.shipConfigs = shipConfigs;
                    return gameConfig;
                }
            }

            gameConfig.N = -1;
            return gameConfig;
        }
    }
}
