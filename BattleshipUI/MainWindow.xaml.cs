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
    public interface IClearable
    {
        void Clear();
    }

    public partial class MainWindow : Window, IClearable
    {
        public BattleInfo BattleInfo { get { return this.BattleInfoControl; } }

        public MainWindow()
        {
            InitializeComponent();

            this.MainGrid.PreviewKeyUp += MainGrid_PreviewKeyUp;
            this.MainGrid.PreviewKeyDown += MainGrid_PreviewKeyDown;
        }



        public void BuildGround(int n, Owner owner)
        {
            Grid ground = owner == BattleshipUI.Owner.Me ? BattlegroundGrid : Enemy_BattlegroundGrid;

            ground.RowDefinitions.Clear();
            ground.ColumnDefinitions.Clear();
            ground.Children.Clear();
            //ground.ShowGridLines = true;

            for (int i = 0; i < n; i++)
            {
                ground.RowDefinitions.Add(new RowDefinition());
                ground.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Button btn = new Button();
                    if (owner == BattleshipUI.Owner.Enemy)
                        btn.Click += Btn_Click;

                    var sp = new Grid();
                    //sp.Background = new SolidColorBrush(Colors.Aqua);
                    //sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //sp.VerticalAlignment = VerticalAlignment.Stretch;

                    //btn.VerticalAlignment = VerticalAlignment.Stretch;
                    //btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                    btn.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    btn.VerticalContentAlignment = VerticalAlignment.Stretch;

                    btn.BorderThickness = new Thickness(1);

                    btn.Background = new SolidColorBrush(Colors.Gray);
                    btn.Content = sp;
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);

                    ground.Children.Add(btn);
                }
            }
        }

        public void SetMainGridVisibility(bool isVisible)
        {
            if (isVisible)
                MainGrid.Visibility = Visibility.Visible;
            else
                MainGrid.Visibility = Visibility.Hidden;

        }
        public void SetConnectStatus(string status)
        {
            this.ConnectStatusLabel.Content = status;
        }

        public void SetConnectStatusisibility(bool isVisible)
        {
            if (isVisible)
                this.ConnectStatusLabel.Visibility = Visibility.Visible;
            else
                this.ConnectStatusLabel.Visibility = Visibility.Hidden;
        }


        public event EventHandler<RoutedEventArgs> Cell_Click;
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Cell_Click?.Invoke(sender, e);
        }

        private Grid GetCell(int row, int column, Owner owner)
        {
            var children = owner == BattleshipUI.Owner.Me ? BattlegroundGrid.Children : Enemy_BattlegroundGrid.Children;
            foreach (var obj in children)
            {
                var child = (Button)obj;

                if (Grid.GetColumn(child) == column && Grid.GetRow(child) == row)
                {
                    return (Grid)child.Content;
                }
            }
            return null;
        }
        public void SetCellColor(int row, int column, Color color, Owner owner)
        {
            var child = GetCell(row, column, owner);
            if (child != null)
            {
                var button = (Button)child.Parent;
                child.Background = new SolidColorBrush(color);
            }
        }
        public void DrawCross(int row, int column, Owner owner)
        {
            var child = GetCell(row, column, owner);
            if (child != null)
            {
                /*
                <Grid Name="parent">
    <Line  X1="0" Y1="0" X2="{Binding ElementName='parent', Path='ActualWidth'}" Y2="{Binding ElementName='parent', Path='ActualHeight'}" 
           Stroke="Black" StrokeThickness="4" />
    <Line  X1="0" Y1="{Binding ElementName='parent', Path='ActualHeight'}" X2="{Binding ElementName='parent', Path='ActualWidth'}" Y2="0" Stroke="Black" StrokeThickness="4" />
    <Label Background="Red" VerticalAlignment="Center" HorizontalAlignment="Center">My Label</Label>

                */

                Line line1 = new Line();
                line1.X1 = 0;
                line1.Y1 = 0;
                line1.X2 = child.ActualWidth;
                line1.Y2 = child.ActualHeight;
                line1.Stroke = new SolidColorBrush(Colors.Red);
                line1.StrokeThickness = 4;

                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = child.ActualHeight;
                line2.X2 = child.ActualWidth;
                line2.Y2 = 0;
                line2.Stroke = new SolidColorBrush(Colors.Red);
                line2.StrokeThickness = 4;

                Label label = new Label();
                label.Content = "My Label";
                label.Background = new SolidColorBrush(Colors.Green);
                label.VerticalAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = HorizontalAlignment.Center;

                var btn = (Button)child.Parent;
                //btn.Content = line1;
                child.Children.Add(line1);
                child.Children.Add(line2);
                //child.Children.Add(label);
            }
        }
        public void SetSkin(int row, int column, int columnSpan, int rowSpan, Image image, Owner owner)
        {
            var child = GetCell(row, column, owner);
            if (child != null)
            {
                Grid.SetColumn(image, column);
                Grid.SetRow(image, row);

                image.VerticalAlignment = VerticalAlignment.Stretch;
                image.HorizontalAlignment = HorizontalAlignment.Stretch;
                 
                if (columnSpan > 1)
                    Grid.SetColumnSpan(image, columnSpan);
                if (rowSpan > 1)
                    Grid.SetRowSpan(image, rowSpan);

                var grid = owner == BattleshipUI.Owner.Me ? BattlegroundGrid : Enemy_BattlegroundGrid;

                //child.Children.Add(path);
                grid.Children.Add(image);
            }
        }

        public void DrawLineThroughColumn(int row, int column, int columnSpan, int rowSpan, Owner owner)
        {
            var child = GetCell(row, column, owner);
            if (child != null)
            {
                Path path;
                if (rowSpan == 1 && columnSpan > 1)
                    path = GetPath(new Point(0, 0), new Point(1, 0));
                else
                    path = GetPath(new Point(0, 0), new Point(0, 1));

                Grid.SetColumn(path, column);
                Grid.SetRow(path, row);

                if (columnSpan > 1)
                    Grid.SetColumnSpan(path, columnSpan);
                if (rowSpan > 1)
                    Grid.SetRowSpan(path, rowSpan);

                //Grid.SetColumnSpan(path, columnSpan);
                //Grid.SetRowSpan(path, rowSpan);

                var grid = owner == BattleshipUI.Owner.Me ? BattlegroundGrid : Enemy_BattlegroundGrid;

                //child.Children.Add(path);
                grid.Children.Add(path);
            }
        }

        private Path GetPath(Point start, Point end)
        {
            Path path = new Path();
            path.Data = new LineGeometry(start, end);
            path.HorizontalAlignment = HorizontalAlignment.Stretch;
            path.VerticalAlignment = VerticalAlignment.Stretch;
            path.Stretch = Stretch.Fill;
            path.StrokeThickness = 4;
            path.Stroke = Brushes.Black;
            path.Fill = Brushes.MediumSlateBlue;
            return path;
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

        public void Clear()
        {
            this.BattleInfo.Clear();
            this.BattlegroundGrid.Children.Clear();
            this.Enemy_BattlegroundGrid.Children.Clear();
        }
    }
}
