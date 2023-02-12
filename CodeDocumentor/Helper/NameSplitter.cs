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
            List<char> upperGroup = new List<char>();

            for (int i = 0; i < name.Length; i++)
            {
                var lookahead = i + 1;

                //search for whole group of uppercase
                char nextChar = name[i];
                upperGroup.Clear();
                while (char.IsUpper(nextChar))
                {
                    upperGroup.Add(nextChar);
                    if(lookahead >= name.Length)
                    {
                        break;
                    }
                    nextChar = name[lookahead];
                    lookahead++;
                }
                if (upperGroup.Count > 1)
                {
                    //take previous and set as word
                    words.TryAddSingleWord(singleWord, true);
                    //Grab the group minus the last one which is the start of the next word
                    var grab = lookahead >= name.Length ? upperGroup.Count : upperGroup.Count - 1;
                    singleWord.AddRange(upperGroup.GetRange(0, grab));
                    words.TryAddSingleWord(singleWord, true);

                    if (lookahead >= name.Length)
                    {
                        //we are at the end
                        break;
                    }
                    else
                    {
                        i = lookahead - 2; //reset back to start of next Uppercase word
                        ProcessChar(i, name, ref singleWord, ref words);
                    }
                }
                else
                {
                    ProcessChar(i, name, ref singleWord, ref words);
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
