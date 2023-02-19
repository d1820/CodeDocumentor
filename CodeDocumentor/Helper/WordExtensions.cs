using System;
using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public static class WordExtensions
    {
        public static void TryAddSingleWord(this List<string> words, List<char> singleWord, bool clearSingleWord = false)
        {
            if (singleWord.Any())
            {
                words.Add(new string(singleWord.ToArray()));
            }
            if (clearSingleWord)
            {
                singleWord.Clear();
            }
        }

        public static bool IsVerbCombo(this string word, string nextWord = null)
        {
            var skipWord = Constants.INTERNAL_SPECIAL_WORD_LIST.Any(w => w.Equals(word, System.StringComparison.InvariantCultureIgnoreCase) || (w + "ed").Equals(word, System.StringComparison.InvariantCultureIgnoreCase));
            var skipNextWord = false;
            if (!string.IsNullOrEmpty(nextWord))
            {
                skipNextWord = Constants.INTERNAL_SPECIAL_WORD_LIST.Any(w => w.Equals(nextWord, System.StringComparison.InvariantCultureIgnoreCase) || (w + "ed").Equals(nextWord, System.StringComparison.InvariantCultureIgnoreCase));
            }
            return skipWord || skipNextWord;
        }

        public static bool IsBoolReturnType(this TypeSyntax returnType)
        {
            return returnType.ToString().IndexOf("bool", StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
