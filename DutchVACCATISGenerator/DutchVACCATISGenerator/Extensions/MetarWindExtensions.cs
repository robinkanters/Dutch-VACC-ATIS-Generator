using DutchVACCATISGenerator.Types;

namespace DutchVACCATISGenerator.Extensions
{
    public static class MetarWindExtensions
    {
        /// <summary>
        /// Process wind string to MetarWind.
        /// </summary>
        /// <param name="input">Wind to process.</param>
        /// <returns>New MetarWind which represents the wind with a heading and strength.</returns>
        public static MetarWind GetMetarWind(this string input)
        {
            MetarWind metarWind;

            if (input.Contains("G"))
                metarWind = new MetarWind(input.Substring(0, 3), input.Substring(3, 2), input.Substring(6, 2));

            else if (input.Substring(3, 1).Equals("0"))
                metarWind = new MetarWind(input.Substring(0, 3), input.Substring(4, 1));
            else
                metarWind = new MetarWind(input.Substring(0, 3), input.Substring(3, 2));

            return metarWind;
        }

        /// <summary>
        /// Process calm wind string to MetarWind.
        /// </summary>
        /// <param name="input">Wind to process.</param>
        /// <returns>New MetarWind which represents a calm wind.</returns>
        public static MetarWind GetMetarWindCalm(this string input)
        {
            return new MetarWind(true, input.Substring(3, 2));
        }

        /// <summary>
        /// Process variable wind string to MetarWind.
        /// </summary>
        /// <param name="metarWind">MetarWind to process.</param>
        /// <param name="input">Variable wind to process.</param>
        /// <returns>MetarWind with variable fields set.</returns>
        public static MetarWind GetMetarWindVariable(this string input, MetarWind metarWind)
        {
            metarWind.WindVariableLeft = input.Substring(0, 3);
            metarWind.WindVariableRight = input.Substring(4, 3);

            return metarWind;
        }
    }
}
