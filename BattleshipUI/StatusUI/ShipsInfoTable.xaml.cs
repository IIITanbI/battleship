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
            public ShipUiConfig ShipUiConfig { get; set; }
            public Label SkinElement { get; set; }
            public Label CountElement { get; set; }
        }
        private Dictionary<int, GridConfig> _config = new Dictionary<int, GridConfig>();

        public ShipsInfoTable()
        {
            InitializeComponent();
        }

        public void Generate(List<ShipUiConfig> configs)
        {
            foreach (var config in configs)
            {
                int id = config.ID;

                MainGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
                int lastRow = MainGrid.RowDefinitions.Count - 1;

                var skinElement = GetSkinElement();
                var countElement = GetCountElement();

                Grid.SetRow(skinElement, lastRow);
                Grid.SetColumn(skinElement, 0);

                Grid.SetRow(countElement, lastRow);
                Grid.SetColumn(countElement, 1);

                MainGrid.Children.Add(skinElement);
                MainGrid.Children.Add(countElement);


                GridConfig gridConfig = new GridConfig()
                {
                    ShipUiConfig = config,
                    CountElement = countElement,
                    SkinElement = skinElement
                };

                _config[id] = gridConfig;
                Redraw(id);
            }
        }

        public void SetCount(int id, int count)
        {
            if (!_config.ContainsKey(id)) return;

            _config[id].ShipUiConfig.Count = count;
            Redraw(id);
        }

        private void Redraw(int id)
        {
            if (!_config.ContainsKey(id)) return;

            var gridConfig = _config[id];

            gridConfig.SkinElement.Content = gridConfig.ShipUiConfig.Skin;
            gridConfig.CountElement.Content = gridConfig.ShipUiConfig.Count;
        }

        private Label GetSkinElement()
        {
            var skinElement = new Label();
            return skinElement;
        }
        private Label GetCountElement()
        {
            var countElement = new Label();
            return countElement;
        }
    }
}
