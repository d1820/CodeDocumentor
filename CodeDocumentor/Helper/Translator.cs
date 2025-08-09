using System.Text.RegularExpressions;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor.Helper
{
    public static class Translator
    {
        private static IOptionsService _optionsService;

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this CSharpSyntaxNode node)
        {
            return TranslateText(node.ToString());
        }

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this string text)
        {
            return TranslateText(text);
        }

        public static void Initialize(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        internal static string TranslateText(string text)
        {
            if (_optionsService == null)
            {
                return text;
            }
            var converted = text;
            if (_optionsService.WordMaps == null)
            {
                return converted;
            }
            converted = converted.SwapXmlTokens((line) =>
            {
                foreach (var wordMap in _optionsService.WordMaps)
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
