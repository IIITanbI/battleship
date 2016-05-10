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
    public partial class BattleInfo : UserControl, IClearable
    {
        public ShipsInfoTable MyShipsTable { get { return this.MyShips; } }
        public ShipsInfoTable EnemyShipsTable { get { return this.EnemyShips; } }

        public BattleInfo()
        {
            InitializeComponent();
        }

        public void SetStartButtonEnabledState(bool enabled)
        {
            this.StartButton.IsEnabled = enabled;
        }
        public void SetRandomButtonEnabledState(bool enabled)
        {
            this.RandomButton.IsEnabled = enabled;
        }

        public void SetTurnStatus(bool isMyTurn)
        {
            if (isMyTurn)
            {
                this.TurnStatusLabel.Content = "Your turn";
            }
            else
            {
                this.TurnStatusLabel.Content = "Enemy's turn";
            }
        }

        public event EventHandler<RoutedEventArgs> ResetButton_Click;
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetButton_Click?.Invoke(sender, e);
        }

        public event EventHandler<RoutedEventArgs> RandomButton_Click;
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            RandomButton_Click?.Invoke(sender, e);
        }

        public event EventHandler<RoutedEventArgs> StartButton_Click;
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton_Click?.Invoke(sender, e);
        }

        public void Clear()
        {
            SetStartButtonEnabledState(false);
            SetRandomButtonEnabledState(true);

            this.MyShips.Clear();
            this.EnemyShips.Clear();
        }
    }
}
