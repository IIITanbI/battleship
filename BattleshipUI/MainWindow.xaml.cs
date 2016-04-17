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
    public enum Owner
    {
        Me,
        Enemy
    }
    public partial class MainWindow : Window
    {

        public BattleInfo BattleInfo { get { return this.BattleInfoControl; } }

        public MainWindow()
        {
            InitializeComponent();

            this.MainGrid.PreviewKeyUp += MainGrid_PreviewKeyUp;
            this.MainGrid.PreviewKeyDown += MainGrid_PreviewKeyDown;
        }

        

        

        public void BuildGround(int n)
        {
            MainGrid.Visibility = Visibility.Visible;

            BattlegroundGrid.RowDefinitions.Clear();
            BattlegroundGrid.ColumnDefinitions.Clear();
            BattlegroundGrid.Children.Clear();
            //BattlegroundGrid.ShowGridLines = true;

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
        }
        public void SetCellColor(int row, int column, Color color)
        {
            foreach (var obj in BattlegroundGrid.Children)
            {
                StackPanel child = (StackPanel)obj;

                if (Grid.GetColumn(child) == column && Grid.GetRow(child) == row)
                {
                    child.Background = new SolidColorBrush(color);
                    break;
                }
            }
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

        public event EventHandler<KeyEventArgs> BattlegroundGrid_KeyPress;
        private void MainGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            BattlegroundGrid_KeyPress?.Invoke(sender, e);
        }
        private void MainGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

    }
}
