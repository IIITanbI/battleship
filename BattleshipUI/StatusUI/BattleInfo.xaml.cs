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
    /// <summary>
    /// Interaction logic for BattleInfo.xaml
    /// </summary>
    public partial class BattleInfo : UserControl
    {
        public BattleInfo()
        {
            InitializeComponent();
        }

        public ShipsTable MyShipsTable { get; private set; }
        public ShipsTable EnemyShipsTable { get; private set; }
        public void Generate(Dictionary<int, int> myShips, Dictionary<int, int> enemyShips)
        {
            MyShipsTable = new ShipsTable(myShips);
            EnemyShipsTable = new ShipsTable(enemyShips);
        }
    }
}
