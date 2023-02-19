using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   The name splitter.
    /// </summary>
    public static class NameSplitter
    {
        private static Regex IsUpperRegEx = new Regex(@"^[^a-z]*$", RegexOptions.Compiled);
        private static Regex IsLowerRegEx = new Regex(@"^[^A-Z]*$", RegexOptions.Compiled);
        private static Regex SpecailCharRegEx = new Regex(@"[^\@_]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Checks if is all upper case.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A bool.</returns>
        public static bool IsAllUpperCase(string text)
        {
            return IsUpperRegEx.IsMatch(text);
        }

        /// <summary>
        ///  Checks if is all lower case.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A bool.</returns>
        public static bool IsAllLowerCase(string text)
        {
            return IsLowerRegEx.IsMatch(text);
        }

        /// <summary>
        /// Upper the to title case.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns><![CDATA[IEnumerable<char>]]></returns>
        public static IEnumerable<char> UpperToTitleCase(string text)
        {
            bool newWord = true;
            foreach (char c in text)
            {
                if (newWord) { yield return Char.ToUpper(c); newWord = false; }
                else yield return Char.ToLower(c);
                if (c == ' ') newWord = true;
            }
        }

        /// <summary>
        ///   Splits name by upper character.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A list of words. </returns>
        public static List<string> Split(string name)
        {
            List<string> words = new List<string>();
            List<char> singleWord = new List<char>();
            List<char> upperGroup = new List<char>();

            if (IsAllUpperCase(name) || IsAllLowerCase(name))
            {
                var matches = SpecailCharRegEx.Matches(name);
                foreach (Match m in matches)
                {
                    if (m.Success && !string.IsNullOrWhiteSpace(m.Value))
                    {
                        words.Add(new string(UpperToTitleCase(m.Value).ToArray()));
                    }
                }
                return words;
            }

            var splitName = name.AsSpan();

            for (int i = 0; i < splitName.Length; i++)
            {
                var lookahead = i + 1;

                //search for whole group of uppercase
                char nextChar = splitName[i];
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
        /// Processes the char.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="name">The name.</param>
        /// <param name="singleWord">The single word.</param>
        /// <param name="words">The words.</param>
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
