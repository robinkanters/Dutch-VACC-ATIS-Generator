using DutchVACCATISGenerator.Resources;

namespace DutchVACCATISGenerator.Extensions
{
    public static class PhenomenaExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phenomena"></param>
        /// <returns></returns>
        public static string PhenomenaToFullSpelling(this string phenomena)
        {
            return Phenomena.ResourceManager.GetString(phenomena);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phenomena"></param>
        /// <returns></returns>
        public static bool IsPhenomena(this string phenomena)
        {
            return !string.IsNullOrWhiteSpace(Phenomena.ResourceManager.GetString(phenomena)) || phenomena.StartsWith("-") || phenomena.StartsWith("+");
        }
    }
}
