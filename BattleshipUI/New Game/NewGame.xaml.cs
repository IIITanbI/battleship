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
using System.Windows.Shapes;

namespace BattleshipUI.New_Game
{

    public partial class NewGame : Window
    {
        private class GridConfig
        {
            public Label SkinElement { get; set; }
            public TextBox CountElement { get; set; }
        }
        private Dictionary<int, GridConfig> _config = new Dictionary<int, GridConfig>();

        public NewGame()
        {
            InitializeComponent();
        }

        public string ResponseTextBoxText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        //id -> maxPossibleCount
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

                countElement.TextInput += CountElement_TextInput;

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
    
            }
        }

        public event EventHandler<TextCompositionEventArgs> TextChanged;
        private void CountElement_TextInput(object sender, TextCompositionEventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }

        public void SetCount(int id, int count)
        {
            if (!_config.ContainsKey(id)) return;

            if (count == -1)
                _config[id].CountElement.Text = "?";
            else
                _config[id].CountElement.Text = count.ToString();
        }
        public int GetCount(int id)
        {
            if (!_config.ContainsKey(id)) throw new ArgumentException("id was not found");

            int value;
            if (int.TryParse(_config[id].CountElement.Text, out value))
            {
                return value;
            }
            else return -1;
        }

        private Button GetSkinElement()
        {
            Button button = new Button();
            var skinElement = new Label();
            button.Content = skinElement;
            return button;
        }
        private TextBox GetCountElement()
        {
            var countElement = new TextBox();
            return countElement;
        }
        
        public event EventHandler<RoutedEventArgs> OkButton_Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OkButton_Click?.Invoke(this, e);
        }

        private void ResponseTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string res = "";
            foreach (var ch in ResponseTextBox.Text)
            {
                if (char.IsDigit(ch))
                    res += ch;
            }
            ResponseTextBox.Text = res;
            ResponseTextBox.SelectionStart = Math.Max(ResponseTextBox.Text.Length, 0); // add some logic if length is 0
            ResponseTextBox.SelectionLength = 0;
            e.Handled = true;
        }
    }
}
