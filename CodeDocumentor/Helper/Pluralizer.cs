using System;
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
        internal static ThirdPartPluralizer.Pluralizer pl = new ThirdPartPluralizer.Pluralizer();

        static Pluralizer()
        {
            pl.AddIrregularRule("Do", "Does");
            pl.AddIrregularRule("To", "Converts to");

        }
        /// <summary>
        ///   Pluralizes word.
        /// </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A plural word. </returns>
        public static string Pluralize(string word)
        {
            var skipPlural = word.IsVerbCombo();
            var pluarlizeAnyway = Constants.PLURALIZE_EXCLUSION_LIST.Any(w => w.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (!skipPlural || pluarlizeAnyway)
            {
                return pl.Pluralize(word);
            }
            return word;
        }

        public static string Pluralize(string word, string nextWord)
        {
            var skipPlural = word.IsVerbCombo(nextWord);
            var pluarlizeAnyway = Constants.PLURALIZE_EXCLUSION_LIST.Any(w => w.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (!skipPlural || pluarlizeAnyway)
            {
                return pl.Pluralize(word);
            }
            return word;

        }
    }
}
