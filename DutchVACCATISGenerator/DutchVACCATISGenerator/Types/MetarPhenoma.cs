namespace DutchVACCATISGenerator.Types
{
    /// <summary>
    /// Represents the phenomena of a METAR as easy accessible fields.
    /// </summary>
    public class MetarPhenomena
    {
        public bool Light { get; set; }
        public bool Heavy { get; set; }
        public string Phenomena { get; set; }

        /// <summary>
        /// Constructs a MetarPhenomena.
        /// </summary>
        /// <param name="phenoma">Phenomena observed.</param>
        public MetarPhenomena(string phenoma)
        {
            Phenomena = phenoma;
        }

        /// <summary>
        /// Constructs a MetarPhenomena with intensity.
        /// </summary>
        /// <param name="light"></param>
        /// <param name="heavy"></param>
        /// <param name="phenomena">Phenomena observed.</param>
        public MetarPhenomena(bool light, bool heavy, string phenomena)
        {
            Light = light;
            Heavy = heavy;
            Phenomena = phenomena;
        }
    }
}
