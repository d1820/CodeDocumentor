using System;
using System.Linq;
using CodeDocumentor.Vsix2022;

namespace CodeDocumentor.Helper
{
    /// <summary> The pluralizer to pluralize word. </summary>
    public static class Pluralizer
    {
        static Pluralizer()
        {
            foreach (var item in Constants.INTERNAL_WORD_MAPS)
            {
                pl.UpsertIrregularRule(item.Word, item.Translation);
            }
        }

        /// <summary> Is plural. </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A bool. </returns>
        public static bool IsPlural(string word) => pl.IsPlural(word);

        /// <summary> Pluralizes word. </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A plural word. </returns>
        public static string Pluralize(string word)
        {
            return Pluralize(word, null);
        }

        public static string Pluralize(string word, string nextWord)
        {
            var skipPlural = word.IsVerbCombo(nextWord);
            var pluarlizeAnyway = Constants.PLURALIZE_ANYWAY_LIST().Any(w => w.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (!skipPlural || pluarlizeAnyway)
            {
                return pl.Pluralize(word);
            }
            return word;
        }

        public static string PluralizeCustom(string word, string nextWord = null)
        {
            var convertCustom = Constants.PLURALIZE_CUSTOM_LIST.FirstOrDefault(f => f.Word.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (convertCustom == null)
            {
                return word;
            }
            return convertCustom.GetTranslation(nextWord);
        }

        internal static CustomPluralizer pl = new CustomPluralizer();
    }
}
