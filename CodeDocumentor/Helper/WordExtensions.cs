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
        private static readonly Regex _xmlElementRegEx = new Regex(Constants.XML_ELEMENT_ONLY_MATCH_REGEX_TEMPLATE);

        public static string Clean(this string word)
        {
            var pattern = "[^a-zA-Z0-9 ]";
            return Regex.Replace(word, pattern, "");
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

        public static bool IsBoolReturnType(this TypeSyntax returnType)
        {
            return returnType.ToString().IndexOf("bool", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static bool IsPastTense(this string word)
        {
            // Check if the word ends with "-ed"
            return word.EndsWith("ed");
        }

        public static bool IsIngVerb(this string word)
        {
            // Check if the word ends with "-ed"
            return word.EndsWith("ing") && !word.Equals("string", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsTwoLetterPropertyExclusionVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            return Constants.TWO_LETTER_PROPERTY_WORD_EXCLUSION_LIST.Any(w => w.Equals(checkWord, System.StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsTwoLetterVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            return Constants.TWO_LETTER_WORD_LIST.Any(w => w.Equals(checkWord, System.StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsVerb(this string word)
        {
            var checkWord = word.GetWordFirstPart().Clean();
            var variations = new List<string>();
            var baseWord = checkWord;
            if (checkWord.IsIngVerb())
            {
                return true;
            }
            else if (Constants.PAST_TENSE_WORDS_NOT_VERBS.Any(a => a.Equals(checkWord, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }
            else if (baseWord.IsPastTense()) //remove "ed"
            {
                baseWord = baseWord.Substring(0, checkWord.Length - 2);
            }
            else if (baseWord.EndsWith("s") && !Constants.LETTER_S_SUFFIX_EXCLUSION_FOR_PLURALIZER.Any(a => a.Equals(baseWord, StringComparison.InvariantCultureIgnoreCase)))
            {
                baseWord = baseWord.Substring(0, checkWord.Length - 1);
            }

            return Constants.GetInternalVerbCheckList().Any(w =>
                w.Equals(baseWord, System.StringComparison.InvariantCultureIgnoreCase)
                || checkWord.Equals((w + "ed"), System.StringComparison.InvariantCultureIgnoreCase)
                //|| checkWord.Equals((w + "ing"), System.StringComparison.InvariantCultureIgnoreCase)
                || (checkWord.Equals((w + "s"), System.StringComparison.InvariantCultureIgnoreCase)
                    && !Constants.LETTER_S_SUFFIX_EXCLUSION_FOR_PLURALIZER.Any(a => a.Equals(w, StringComparison.InvariantCultureIgnoreCase)))
            );
        }

        public static bool IsVerbCombo(this string word, string nextWord = null)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }
            var skipWord = word.IsVerb();
            var skipNextWord = false;
            if (!string.IsNullOrEmpty(nextWord) && !skipWord)
            {
                skipNextWord = nextWord.IsVerb();
            }
            return skipWord || skipNextWord;
        }

        public static bool IsXml(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            return _xmlElementRegEx.IsMatch(str);
        }

        public static bool StartsWith_A_An_And(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            return str.StartsWith("a ", StringComparison.InvariantCultureIgnoreCase) ||
                str.StartsWith("an ", StringComparison.InvariantCultureIgnoreCase) ||
                str.StartsWith("and ", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string ToTitleCase(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return txt;
            }

            return char.ToUpper(txt[0]) + txt.Substring(1);
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
