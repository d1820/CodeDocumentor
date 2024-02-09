using System;
using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public static class WordExtensions
    {
        public static bool IsBoolReturnType(this TypeSyntax returnType)
        {
            return returnType.ToString().IndexOf("bool", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static bool IsVerbCombo(this string word, string nextWord = null)
        {
            var skipWord = word.IsVerb();
            var skipNextWord = false;
            if (!string.IsNullOrEmpty(nextWord))
            {
                skipNextWord = nextWord.IsVerb();
            }
            return skipWord || skipNextWord;
        }

        public static bool IsPastTense(this string word)
        {
            // Check if the word ends with "-ed"
            if (word.EndsWith("ed"))
            {
                return true;
            }
            // Add additional checks for irregular verbs here if needed
            // Example:
            // else if (word == "ate" || word == "ran")
            // {
            //     return true;
            // }
            else
            {
                return false;
            }
        }

        public static bool IsVerb(this string word)
        {
            var baseWord = word.EndsWith("ing") ? word.Substring(0, word.Length - 3) : word;
            baseWord = baseWord.EndsWith("ed") ? baseWord.Substring(0, word.Length - 2) : baseWord;
            baseWord = baseWord.EndsWith("s") ? baseWord.Substring(0, word.Length - 1) : baseWord;
            return Constants.GetInternalVerbCheckList().Any(w => w.Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || (w + "ed").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || (w + "ing").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || (w + "s").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase));
        }

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
    }
}
