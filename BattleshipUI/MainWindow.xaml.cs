using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattleshipUI
{
    public class GameSettings
    {
        public List<Ship>
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StartNewGame();
        }

        public void StartNewGame()
        {
            NewGame game = new NewGame();
            if (game.ShowDialog() == true)
            {
                int n = (int)game.Tag;
                BuildGround(n);
            }
        }

        Battleground ground;

        public void BuildGround(int n)
        {
            ground = new Battleground(n);

            for (int i = 0; i < n; i++)
            {
                RowDefinition row = new RowDefinition();
                BattlegroundGrid.RowDefinitions.Add(row);

                ColumnDefinition column = new ColumnDefinition();
                BattlegroundGrid.ColumnDefinitions.Add(column);
            }
            BattlegroundGrid.ShowGridLines = true;
            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    StackPanel sp = new StackPanel();
                    sp.Background = new SolidColorBrush(Colors.Aqua);
                    Grid.SetRow(sp, i);
                    Grid.SetColumn(sp, j);

                    BattlegroundGrid.Children.Add(sp);
                }
            }


        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}
