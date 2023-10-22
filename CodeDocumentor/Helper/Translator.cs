using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor.Helper
{
    public static class Translator
    {
        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string Translate(this CSharpSyntaxNode node)
        {
            return TranslateText(node.ToString());
        }

        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string Translate(this string text)
        {
            return TranslateText(text);
        }

        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string TranslateText(string text)
        {
            string converted = text;
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.WordMaps == null)
            {
                //Some stuff just needs to be handled for the user
                foreach (var wordMap in Constants.INTERNAL_WORD_MAPS)
                {
                    var wordToLookFor = string.Format(wordMatchRegexTemplate, wordMap.Word);
                    converted = Regex.Replace(converted, wordToLookFor, wordMap.GetTranslation());
                }
                return converted;
            }

            var mergedWorkMaps = new HashSet<WordMap>(optionsService.WordMaps);
            //Some stuff just needs to be handled for the user
            foreach (var item in Constants.INTERNAL_WORD_MAPS)
            {
                mergedWorkMaps.Add(item);
            }
            foreach (var wordMap in mergedWorkMaps)
            {
                var wordToLookFor = string.Format(wordMatchRegexTemplate, wordMap.Word);
                converted = Regex.Replace(converted, wordToLookFor, wordMap.GetTranslation());
            }
            return converted;
        }

        private static readonly string wordMatchRegexTemplate = @"\b({0})\b";
    }
}
