using System;
using System.Collections.Generic;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   The name splitter.
    /// </summary>
    public class NameSplitter
    {
        /// <summary>
        ///   Splits name by upper character.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A list of words. </returns>
        public static List<string> Split(ReadOnlySpan<char> name)
        {
            List<string> words = new List<string>();
            List<char> singleWord = new List<char>();

            foreach (char c in name)
            {
                if (char.IsUpper(c) && singleWord.Count > 0)
                {
                    words.Add(new string(singleWord.ToArray()));
                    singleWord.Clear();
                    singleWord.Add(c);
                }
                else
                {
                    singleWord.Add(c);
                }
            }

            words.Add(new string(singleWord.ToArray()));

            return words;
        }
    }
}
