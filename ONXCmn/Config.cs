using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ONXCmn.Logic
{
    [Serializable]
    public class GameConfig : ICloneable
    {
        //ground size
        public int N { get; set; }

        //ship config
        public List<ShipConfig> shipConfigs { get; set; } = new List<ShipConfig>();

        public object Clone()
        {
            var gameConfig = new GameConfig();
            gameConfig.N = N;
            gameConfig.shipConfigs = shipConfigs.Select(sc => sc.Clone() as ShipConfig).ToList();
            return gameConfig;
        }
    }

    [Serializable]
    public class ShipConfig : ICloneable
    {
        public int ID { get; set; }
        public int Length { get; set; }
        public int Count { get; set; }

        public object Clone()
        {
            var shipConfig = new ShipConfig();
            shipConfig.ID = this.ID;
            shipConfig.Length = this.Length;
            shipConfig.Count = this.Count;
            return shipConfig;
        }
    }
}
