using System.Text.RegularExpressions;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor.Helper
{
    public static class Translator
    {

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this CSharpSyntaxNode node, WordMap[] wordMaps)
        {
            return TranslateText(node.ToString(), wordMaps);
        }

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this string text, WordMap[] wordMaps)
        {
            return TranslateText(text, wordMaps);
        }

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        private static string TranslateText(string text, WordMap[] wordMaps)
        {
            var converted = text;
            if (wordMaps == null ||  wordMaps.Length == 0)
            {
                return converted;
            }
            converted = converted.SwapXmlTokens((line) =>
            {
                foreach (var wordMap in wordMaps)
                {
                    var wordToLookFor = string.Format(Constants.WORD_MATCH_REGEX_TEMPLATE, wordMap.Word);
                    line = Regex.Replace(line, wordToLookFor, wordMap.GetTranslation());
                }
                return line;
            });
            return converted;
        }
    }
}
