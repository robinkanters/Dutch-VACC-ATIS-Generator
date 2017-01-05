using System;
using System.Collections.Generic;

namespace DutchVACCATISGenerator.Types
{
    /// <summary>
    /// Represents the fields of a METAR.
    /// </summary>
    public class Metar
    {
        public string ICAO { get; set; }
        public string Time { get; set; }
        public MetarWind Wind { get; set; }
        public List<MetarPhenomena> Phenomena { get; set; }
        public bool CAVOK { get; set; }
        public int Visibility { get; set; }
        public bool RVR { get; set; }
        public Dictionary<string, int> RVRValues { get; set; }
        public int VerticalVisibility { get; set; }
        public bool SKC { get; set; }
        public bool NSC { get; set; }
        public List<MetarCloud> Clouds { get; set; }
        public string Temperature { get; set; }
        public string Dewpoint { get; set; }
        public int QNH { get; set; }
        public bool NOSIG { get; set; }
        public bool NSW { get; set; }
        public MetarBECMG metarBECMG {get; set; }
        public MetarTEMPO metarTEMPO {get; set; }
    
        /// <summary>
        /// Construct a Metar. Initializes fields.
        /// </summary>
        public Metar()
        {
            Phenomena = new List<MetarPhenomena>();
            RVRValues = new Dictionary<string, int>();
            Clouds = new List<MetarCloud>();
        }
    }
}
