namespace DutchVACCATISGenerator.Types
{
    /// <summary>
    /// Represents the cloud of a METAR as easy accessible fields.
    /// </summary>
    public class MetarCloud
    {
        public string CloudType { get; set; }
        public int Altitude { get; set; }
        public string Addition { get; set; }

        /// <summary>
        /// Constructs a MetarCloud with a cloud type and the altitude of the cloud.
        /// </summary>
        /// <param name="cloudType">Type of the cloud.</param>
        /// <param name="altitude">Altitude of the cloud.</param>
        public MetarCloud(string cloudType, int altitude)
        {
            CloudType = cloudType;
            Altitude = altitude;
        }

        /// <summary>
        /// Constructs a MetarCloud with a cloud type, the altitude of the cloud and any addition to the cloud.
        /// </summary>
        /// <param name="cloudType">Type of the cloud.</param>
        /// <param name="altitude">Altitude of the cloud.</param>
        /// <param name="addition">Addition of the cloud.</param>
        public MetarCloud(string cloudType, int altitude, string addition)
        {
            CloudType = cloudType;
            Altitude = altitude;
            Addition = addition;
        }
    }
}
