namespace CodeDocumentor.Helper
{
    public static class Pluralizer
    {
        private static readonly Pluralize.NET.Pluralizer _netPluralizer;

        static Pluralizer()
        {
            //foreach (var item in Constants.INTERNAL_WORD_MAPS)
            //{
            //    UpsertIrregularRule(item.Word, item.Translation);
            //}
            _netPluralizer = new Pluralize.NET.Pluralizer();
            _netPluralizer.AddIrregularRule("error", "error");
        }

        public static bool IsPlural(string word)
        {
            return _netPluralizer.IsPlural(word);
        }

        public static string ForcePluralization(string word)
        {
            return _netPluralizer.Pluralize(word);
        }

        /// <summary> Pluralizes word. </summary>
        /// <param name="word"> The word. </param>
        /// <returns> A plural word. </returns>
        public static string Pluralize(string word)
        {
            return Pluralize(word, null);
        }

        public static string Pluralize(string word, string nextWord)
        {
            var skipPlural = word.IsVerbCombo(nextWord); //we dont pluralize first work verb of if the second word is a verb
            //var pluarlizeAnyway = Constants.PLURALIZE_ANYWAY_LIST().Any(w => w.Equals(word, StringComparison.InvariantCultureIgnoreCase));
            if (!skipPlural) //|| pluarlizeAnyway
            {
                var checkWord = word.GetWordFirstPart();
                var pluraled = _netPluralizer.Pluralize(checkWord);
                word = word.Replace(checkWord, pluraled);
            }
            return word;
        }



        //public string PluralizeCustom(string word, string nextWord = null)
        //{
        //    var convertCustom = Constants.PLURALIZE_CUSTOM_LIST.FirstOrDefault(f => f.Word.Equals(word, StringComparison.InvariantCultureIgnoreCase));
        //    if (convertCustom == null)
        //    {
        //        return word;
        //    }
        //    return convertCustom.GetTranslation(nextWord);
        //}
    }
}
