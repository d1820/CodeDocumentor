#pragma warning disable IDE0130

namespace System
{
    public static class StringExtensions
    {
        public static string RemovePeriod(this string text)
        {
            return text?.Trim().EndsWith(".") == true ? text.Remove(text.Length - 1) : text;
        }

        /// <summary>
        ///  Withs the period.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> A string. </returns>
        public static string WithPeriod(this string text)
        {
            if (text?.Trim().EndsWith(".") == true)
            {
                return text;
            }
            return text.Length > 0 ? text + "." : text;
        }
    }
}
