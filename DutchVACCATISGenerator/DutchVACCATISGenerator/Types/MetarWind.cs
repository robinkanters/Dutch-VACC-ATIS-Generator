namespace DutchVACCATISGenerator.Types
{
    /// <summary>
    /// Represents the wind of a METAR as easy accessible fields.
    /// </summary>
    public class MetarWind
    {
        public bool Vrb { get; set; }
        public string WindHeading { get; set; }
        public string WindKnots { get; set; }
        public string WindGustMin { get; set; }
        public string WindGustMax { get; set; }
        public string WindVariableLeft { get; set; }
        public string WindVariableRight { get; set; }

        /// <summary>
        /// Constructs a MetarWind with a variable wind and wind strength.
        /// </summary>
        /// <param name="vrb">Indicates that the wind is variable.</param>
        /// <param name="windKnots">Wind strength (knots).</param>
        public MetarWind(bool vrb, string windKnots)
        {
            Vrb = vrb;
            WindKnots = windKnots;
        }

        /// <summary>
        /// Constructs a MetarWind with the wind heading and wind strength.
        /// </summary>
        /// <param name="windHeading">Heading of the wind.</param>
        /// <param name="windKnots">Wind strength (knots).</param>
        public MetarWind(string windHeading, string windKnots)
        {
            WindHeading = windHeading;
            WindKnots = windKnots;
        }

        /// <summary>
        /// Constructs a MetarWind with a wind heading, minimal wind gust and maximum wind gust.
        /// </summary>
        /// <param name="windHeading">Heading of the wind.</param>
        /// <param name="windGustMin">Minimal speed of the wind (knots).</param>
        /// <param name="windGustMax">Maximum speed of the wind (knots).</param>
        public MetarWind(string windHeading, string windGustMin, string windGustMax)
        {
            WindHeading = windHeading;
            WindGustMin = windGustMin;
            WindGustMax = windGustMax;
        }
    }
}
