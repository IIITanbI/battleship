using BattleshipUI.New_Game;
using ONXCmn.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Controller
{
    public class NewGameController
    {
        private NewGame newGame;


        private int N
        {
            get
            {
                string text = newGame.ResponseTextBoxText;
                int n;

                if (!int.TryParse(text, out n) ||
                   n < 10 || n > 20)
                {
                    MessageBox.Show("Invalid value", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return n;
            }
        }
        private void NewGame_OkButton_Click(object sender, RoutedEventArgs e)
        {
            int t = this.N;
            newGame.DialogResult = true;
        }
        List<ShipConfig> configs = new List<ShipConfig>();

        public GameConfig StartNewGame(List<ShipConfig> configs)
        {
            GameConfig gameConfig = new GameConfig();

            //List<ShipConfig> shipConfigs = new List<ShipConfig>();
            //for (int length = 4; length <= 4; length++)
            //{
            //    int count = 5 - length;
            //    shipConfigs.Add(new ShipConfig()
            //    {
            //        ID = length,
            //        Count = count,
            //        Length = length
            //    });
            //}
            //gameConfig.N = 15;
            //gameConfig.shipConfigs = shipConfigs;
            //return gameConfig;


            this.configs = configs;
            var ids = this.configs.Select(x => x.ID).ToList();
            newGame = new NewGame();
            newGame.ResponseTextBoxText = "15";
            newGame.TextChanged += NewGame_TextChanged;
            newGame.OkButton_Click += NewGame_OkButton_Click;

            newGame.Generate(ids);

            ids.ForEach(id =>
            {
                int count = this.configs.FirstOrDefault(x => x.ID == id).Count;
                newGame.SetCount(id, count);

                newGame.SetSkin(id, Helper.GetImage(id));
                //Helper.CutImage(UIConfigs.FirstOrDefault(x => x.ShipConfig.ID == id).SkinPath);
            });

            if (newGame.ShowDialog() == true)
            {
                List<ShipConfig> currentConfig = new List<ShipConfig>();
                this.configs.ForEach(config =>
                {
                    var tt = (ShipConfig)config.Clone();
                    tt.Count = newGame.GetCount(config.ID);
                    if (tt.Count > 0)
                        currentConfig.Add(tt);
                });

                newGame.Tag = currentConfig;

                string text = newGame.ResponseTextBoxText;
                int n;
                if (int.TryParse(text, out n))
                {
                    gameConfig.N = n;
                    gameConfig.shipConfigs = currentConfig.ToList();
                    return gameConfig;
                }
            }

            gameConfig.N = -1;
            return gameConfig;
        }

        private void NewGame_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int id = (int)textBox.Tag;
            ShipConfig shipConfig = this.configs.FirstOrDefault(sc => sc.ID == id);
            if (shipConfig == null)
                return;

            int value;
            if (int.TryParse(textBox.Text, out value))
            {
                int total = 0;
                configs.ForEach(config =>
                {
                    total += newGame.GetCount(config.ID);
                });

                if (value < 0)
                {
                    newGame.SetCount(id, shipConfig.Count);
                }
                else
                if (total < this.N)
                {
                    //it's ok
                }
                else
                    newGame.SetCount(id, 0);
            }
            else
            {
                //all bad
                newGame.SetCount(id, shipConfig.Count);
            }
        }
    }

    public static class Helper
    {
        public static Dictionary<int, string> dic = new Dictionary<int, string>();
        public static Dictionary<int, ShipConfig> configs = new Dictionary<int, ShipConfig>();
        public static Image GetImage(int id)
        {
            if (dic.ContainsKey(id))
            {
                string path = dic[id];
                if (path == null || path == "")
                {
                    return null;
                }
                else
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Relative);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    Image image = new Image();
                    image.Source = bitmap;
                    return image;
                }
            }
            else
            {
                throw new ArgumentException("id not found");
            }
        }

        public static void CutImage(string img)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(img, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();


            //var res = new CroppedBitmap(src, new Int32Rect(j * 120, i * 120, 120, 120));
        }
        public static void ParseXml(string path)
        {
            XDocument xml = XDocument.Load(path);
            var configurations = xml.Root.Elements("Configuration");

            foreach (var elem in configurations)
            {
                var id = int.Parse(elem.Attribute("ID").Value);
                var Path = elem.Element("Path").Value;

                var length = int.Parse(elem.Element("Length").Value);
                var count = int.Parse(elem.Element("Count").Value);

                dic[id] = Path;
                configs[id] = new ShipConfig()
                {
                    ID = id,
                    Count = count,
                    Length = length
                };
            }
        }
    }
}
