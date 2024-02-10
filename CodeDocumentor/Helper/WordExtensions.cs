using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (!string.IsNullOrEmpty(nextWord) && !skipWord)
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

        public static bool IsTwoLetterVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            return Constants.TWO_LETTER_WORD_LIST.Any(w => w.Equals(checkWord, System.StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsTwoLetterPropertyExclusionVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            return Constants.TWO_LETTER_PROPERTY_WORD_EXCLUSION_LIST.Any(w => w.Equals(checkWord, System.StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            var variations = new List<string>();
            var baseWord = checkWord;
            if (checkWord.EndsWith("ing") && !checkWord.Equals("string", StringComparison.InvariantCultureIgnoreCase))
            {
                baseWord = word.Substring(0, checkWord.Length - 3);
            }
            else if (baseWord.EndsWith("ed"))
            {
                baseWord = baseWord.Substring(0, checkWord.Length - 2);
            }
            else if (baseWord.EndsWith("s") && !Constants.LETTER_S_SUFFIX_EXCLUSION_FOR_PLURALIZER.Any(a => a.Equals(baseWord, StringComparison.InvariantCultureIgnoreCase)))
            {
                baseWord = baseWord.Substring(0, checkWord.Length - 1);
            }
            return Constants.GetInternalVerbCheckList().Any(w => w.Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || (w + "ed").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || (w + "ing").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
            || ((w + "s").Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
                && !Constants.LETTER_S_SUFFIX_EXCLUSION_FOR_PLURALIZER.Any(a => a.Equals(w, StringComparison.InvariantCultureIgnoreCase)))
            );
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

        public static string ToTitleCase(this string txt)
        {
            return char.ToUpper(txt[0]) + txt.Substring(1);
        }

        public static string GetWordFirstPart(this string word)
        {
            var checkWord = word;
            if (word.Contains(" ")) //a translation already happened and swapped a word for a set of words. the first word is really what we are checking
            {
                checkWord = word.Split(' ').First();
            }
            return checkWord;
        }

        public static string Clean(this string word)
        {
            string pattern = "[^a-zA-Z0-9 ]";
            return Regex.Replace(word, pattern, "");
        }
    }
}
