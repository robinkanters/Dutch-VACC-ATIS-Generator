using System.Linq;

namespace DutchVACCATISGenerator.Extensions
{
    public static class StringExstenions
    {
        /// <summary>
        /// Checks if the input string is numbers only.
        /// </summary>
        /// <param name="input">String to check.</param>
        /// <returns>Boolean indicating if the string is numbers only.</returns>
        public static bool IsNumbersOnly(this string input)
        {
            return input.All(char.IsDigit);
        }

        /// <summary>
        /// Checks if the input string is the given length.
        /// </summary>
        /// <param name="input">String to check.</param>
        /// /// <param name="lenght">Length to check string to.</param>
        /// <returns>Boolean indicating if the string length is the length.</returns>
        public static bool IsLength(this string input, int lenght)
        {
            return input.Length == lenght;
        }
    }
}
