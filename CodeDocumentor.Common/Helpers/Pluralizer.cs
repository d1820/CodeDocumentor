#pragma warning disable IDE0130
using CodeDocumentor.Common.Extensions;

namespace System
{
    public static class Pluralizer
    {
        private static readonly Pluralize.NET.Pluralizer _netPluralizer;

        static Pluralizer()
        {
            _netPluralizer = new Pluralize.NET.Pluralizer();
            _netPluralizer.AddIrregularRule("error", "error");
        }

        public static string ForcePluralization(string word)
        {
            return _netPluralizer.Pluralize(word);
        }

        public static bool IsPlural(string word)
        {
            return _netPluralizer.IsPlural(word);
        }

        /// <summary>
        ///  Pluralizes word.
        /// </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A plural word. </returns>
        public static string Pluralize(string word)
        {
            return Pluralize(word, null);
        }

        public static string Pluralize(string word, string nextWord)
        {
            var skipPlural = word.IsVerbCombo(nextWord); //we dont pluralize first work verb of if the second word is a verb
            if (!skipPlural)
            {
                var checkWord = word.GetWordFirstPart();
                var pluraled = _netPluralizer.Pluralize(checkWord);
                word = word.Replace(checkWord, pluraled);
            }
            return word;
        }
    }
}
