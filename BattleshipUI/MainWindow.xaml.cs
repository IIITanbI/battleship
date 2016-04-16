using BattleshipUI.New_Game;
using BattleshipUI.StatusUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void BuildGround(int n, List<ShipUiConfig> config)
        {
            MainGrid.Visibility = Visibility.Visible;

            BattlegroundGrid.RowDefinitions.Clear();
            BattlegroundGrid.ColumnDefinitions.Clear();
            BattlegroundGrid.Children.Clear();
            BattlegroundGrid.ShowGridLines = true;

            for (int i = 0; i < n; i++)
            {
                BattlegroundGrid.RowDefinitions.Add(new RowDefinition());
                BattlegroundGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    StackPanel sp = new StackPanel();
                    sp.Background = new SolidColorBrush(Colors.Aqua);
                    Grid.SetRow(sp, i);
                    Grid.SetColumn(sp, j);

                    BattlegroundGrid.Children.Add(sp);
                }
            }

            BattleInformation.MyShips.Generate(config);
        }

        public event EventHandler NewGameButton_Click;
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameButton_Click?.Invoke(this, null);
        }

        public event EventHandler ConnectButton_Click;
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton_Click?.Invoke(this, null);
        }
    }
}
