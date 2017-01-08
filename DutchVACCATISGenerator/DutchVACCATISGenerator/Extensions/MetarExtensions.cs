using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using DutchVACCATISGenerator.Types;

namespace DutchVACCATISGenerator.Extensions
{
    public static class MetarExtensions
    {
        /// <summary>
        /// Check if input wind string is a variable wind.
        /// </summary>
        /// <param name="input">Wind string.</param>
        /// <returns>Boolean indicating if the wind is variable.</returns>
        public static bool IsVariableWind(this string input)
        {
            return input.Length > 5 && input.Substring(0, 3).All(char.IsDigit) && input.Contains('V') && input.Substring(4).All(char.IsDigit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCloud(this string input)
        {
            return input.StartsWith("FEW") || input.StartsWith("SCT") || input.StartsWith("BKN") || input.StartsWith("OVC");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ATISSamples"></param>
        /// <param name="ICAO"></param>
        public static void AddICAO(this List<string> ATISSamples, string ICAO)
        {
            switch (ICAO)
            {
                //If processed ICAO is EHAM.
                case "EHAM":
                    ATISSamples.Add("ehamatis");
                    break;

                //If processed ICAO is EHBK.
                case "EHBK":
                    ATISSamples.Add("ehbkatis");
                    break;

                //If processed ICAO is EHEH.
                case "EHEH":
                    ATISSamples.Add("ehehatis");
                    break;

                //If processed ICAO is EHGG.
                case "EHGG":
                    ATISSamples.Add("ehggatis");
                    break;

                //If processed ICAO is EHRD.
                case "EHRD":
                    ATISSamples.Add("ehrdatis");
                    break;
            }
        }
    }
}
