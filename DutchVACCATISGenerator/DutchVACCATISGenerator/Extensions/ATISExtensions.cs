using DutchVACCATISGenerator.Resources;

namespace DutchVACCATISGenerator.Extensions
{
    public static class ATISExtensions
    {
        /// <summary>
        /// Returns the capitalized NATO word for the given letter.
        /// </summary>
        /// <param name="atisLetter">Letter</param>
        /// <returns>String - Capitalized NATO word</returns>
        public static string ATISLetterToFullSpelling(this string atisLetter)
        {
            return NATO_phonetic_alphabet.ResourceManager.GetString(atisLetter);
        }
    }
}
