using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ONXCmn.Logic
{
    [Serializable]
    public class GameConfig : MarshalByRefObject
    {
        //ground size
        public int N { get; set; }

        //ship config
        public List<ShipConfig> shipConfigs { get; set; } = new List<ShipConfig>();
    }

    [Serializable]
    public class ShipConfig : MarshalByRefObject
    {
        public int ID { get; set; }
        public int Length { get; set; }
        public int Count { get; set; }
    }
}
