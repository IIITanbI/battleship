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

namespace BattleshipUI.StatusUI
{
    public class ShipUiConfig
    {
        public int ID { get; set; }
        public object Skin { get; set; }
        public int Count { get; set; }
    }
    public partial class ShipsInfoTable : UserControl
    {
        private class GridConfig
        {
            public Label SkinElement { get; set; }
            public Label CountElement { get; set; }
        }

        private Dictionary<int, GridConfig> _config = new Dictionary<int, GridConfig>();

        public ShipsInfoTable()
        {
            InitializeComponent();
        }

        public void Generate(List<int> configsID)
        {
            foreach (var id in configsID)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
                int lastRow = MainGrid.RowDefinitions.Count - 1;

                var skinElement = GetSkinElement(); skinElement.Tag = id;
                var countElement = GetCountElement(); countElement.Tag = id;

                Grid.SetRow(skinElement, lastRow);
                Grid.SetColumn(skinElement, 0);

                Grid.SetRow(countElement, lastRow);
                Grid.SetColumn(countElement, 1);

                MainGrid.Children.Add(skinElement);
                MainGrid.Children.Add(countElement);


                GridConfig gridConfig = new GridConfig()
                {
                    CountElement = countElement,
                    SkinElement = skinElement.Content as Label
                };

                _config[id] = gridConfig;

                skinElement.Click += SkinElement_Click;
            }
        }

        public event EventHandler<RoutedEventArgs> SkinButton_Click;
        private void SkinElement_Click(object sender, RoutedEventArgs e)
        {
            SkinButton_Click?.Invoke(sender, e);
        }

        public void SetCount(int id, int count)
        {
            if (!_config.ContainsKey(id)) return;

            if (count == -1)
                _config[id].CountElement.Content = "?";
            else
                _config[id].CountElement.Content = count;
        }
        public void DisableShipButton(int id)
        {
            if (!_config.ContainsKey(id)) return;
            ((Button)_config[id].SkinElement.Parent).IsEnabled = false;
        }

        

        //public void Redraw(int id)
        //{
        //    if (!_config.ContainsKey(id)) return;

        //    var gridConfig = _config[id];

        //    gridConfig.SkinElement.Content = gridConfig.ShipUiConfig.Skin;
        //    gridConfig.CountElement.Content = gridConfig.ShipUiConfig.Count;
        //}

        private Button GetSkinElement()
        {
            Button button = new Button();
            var skinElement = new Label();
            button.Content = skinElement;
            return button;
        }
        private Label GetCountElement()
        {
            var countElement = new Label();
            return countElement;
        }
    }
}
