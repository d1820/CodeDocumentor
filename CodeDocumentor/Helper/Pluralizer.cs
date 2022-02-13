using System.Linq;
using CodeDocumentor.Vsix2022;
using ThirdPartPluralizer = Pluralize.NET;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   The pluralizer to pluralize word.
    /// </summary>
    public static class Pluralizer
    {
        /// <summary>
        ///   Pluralizes word.
        /// </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A plural word. </returns>
        public static string Pluralize(string word)
        {
            var skipPlural = Constants.INTERNAL_SPECIAL_WORD_LIST.Any(w => w.Equals(word));
            if (!skipPlural)
            {
                var pl = new ThirdPartPluralizer.Pluralizer();
                return pl.Pluralize(word);
            }
            return word;
        }
    }
}
