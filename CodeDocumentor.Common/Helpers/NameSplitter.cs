using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Common.Extensions;

namespace CodeDocumentor.Common
{
    /// <summary>
    ///  The name splitter.
    /// </summary>
    public static class NameSplitter
    {
        private static readonly Regex _isLowerRegEx = new Regex("^[^A-Z]*$", RegexOptions.Compiled);

        private static readonly Regex _isUpperRegEx = new Regex("^[^a-z]*$", RegexOptions.Compiled);

        private static readonly Regex _specailCharRegEx = new Regex(@"[^\@_]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///  Checks if is all lower case.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> A bool. </returns>
        public static bool IsAllLowerCase(string text)
        {
            return _isLowerRegEx.IsMatch(text);
        }

        /// <summary>
        ///  Checks if is all upper case.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> A bool. </returns>
        public static bool IsAllUpperCase(string text)
        {
            return _isUpperRegEx.IsMatch(text);
        }

        /// <summary>
        ///  Splits name by upper character.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A list of words. </returns>
        public static List<string> Split(string name)
        {
            var words = new List<string>();
            var singleWord = new List<char>();
            var upperGroup = new List<char>();

            var allUpperCase = IsAllUpperCase(name);
            var allLowerCase = IsAllLowerCase(name);

            if (allUpperCase || allLowerCase)
            {
                var matches = _specailCharRegEx.Matches(name);
                foreach (Match m in matches)
                {
                    if (m.Success && !string.IsNullOrWhiteSpace(m.Value))
                    {
                        if (allUpperCase)
                        {
                            words.Add(new string(m.Value.ToArray())); // keep casing
                        }
                        else
                        {
                            words.Add(new string(UpperToTitleCase(m.Value).ToArray()));
                        }
                    }
                }
                return words;
            }

            var splitName = name.AsSpan();

            for (var i = 0; i < splitName.Length; i++)
            {
                var lookahead = i + 1;

                //search for whole group of uppercase
                var nextChar = splitName[i];
                upperGroup.Clear();
                while (char.IsUpper(nextChar))
                {
                    upperGroup.Add(nextChar);
                    if (lookahead >= splitName.Length)
                    {
                        break;
                    }
                    nextChar = splitName[lookahead];
                    lookahead++;
                }
                if (upperGroup.Count > 1)
                {
                    //take previous and set as word
                    words.TryAddSingleWord(singleWord, true);
                    //Grab the group minus the last one which is the start of the next word
                    var grab = lookahead >= splitName.Length ? upperGroup.Count : upperGroup.Count - 1;
                    singleWord.AddRange(upperGroup.GetRange(0, grab));
                    words.TryAddSingleWord(singleWord, true);

                    if (lookahead >= splitName.Length)
                    {
                        //we are at the end
                        break;
                    }
                    else
                    {
                        i = lookahead - 2; //reset back to start of next Uppercase word
                        ProcessChar(i, splitName, ref singleWord, ref words);
                    }
                }
                else
                {
                    ProcessChar(i, splitName, ref singleWord, ref words);
                }
            }
            words.TryAddSingleWord(singleWord);
            return words;
        }

        /// <summary>
        ///  Upper the to title case.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> <![CDATA[IEnumerable<char>]]> </returns>
        public static IEnumerable<char> UpperToTitleCase(string text)
        {
            var newWord = true;
            foreach (var c in text)
            {
                if (newWord)
                {
                    yield return char.ToUpper(c);
                    newWord = false;
                }
                else
                {
                    yield return char.ToLower(c);
                }

                if (c == ' ')
                {
                    newWord = true;
                }
            }
        }

        /// <summary>
        ///  Processes the char.
        /// </summary>
        /// <param name="i"> The i. </param>
        /// <param name="name"> The name. </param>
        /// <param name="singleWord"> The single word. </param>
        /// <param name="words"> The words. </param>
        private static void ProcessChar(int i, ReadOnlySpan<char> name, ref List<char> singleWord, ref List<string> words)
        {
            var c = name[i];
            if (char.IsUpper(c) && singleWord.Count > 0)
            {
                if (singleWord.Count > 0)
                {
                    words.Add(new string(singleWord.ToArray()));
                    singleWord.Clear();
                }

                singleWord.Add(c);
            }
            else
            {
                if (c != '_')
                {
                    singleWord.Add(c);
                }
            }
        }
    }
}
