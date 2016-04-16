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
        public NewGame()
        {
            InitializeComponent();
        }

        public string ResponseTextBoxText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
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
