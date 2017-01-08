using System.Collections.Generic;

namespace DutchVACCATISGenerator.Types
{
    public class AbstractMetar
    {
        public bool CAVOK { get; set; }
        public List<MetarCloud> Clouds { get; set; } = new List<MetarCloud>();
        public bool NSW { get; set; }
        public List<MetarPhenomena> Phenomena { get; set; } = new List<MetarPhenomena>();
        public bool SKC { get; set; }
        public int VerticalVisibility { get; set; }
        public int Visibility { get; set; }
        public MetarWind Wind { get; set; }
    }
}