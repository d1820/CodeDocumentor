using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor.Helper
{
    public static class Translator
    {
        public static void Initialize(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this CSharpSyntaxNode node)
        {
            return TranslateText(node.ToString());
        }

        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this string text)
        {
            return TranslateText(text);
        }

        //This translates parts of the comment based on internal maps that should be applied for readabiity
        internal static string InternalTranslateText(this string text, int textPosition)
        {
            var converted = text;
            var userMaps = _optionsService.WordMaps ?? Array.Empty<WordMap>();
            //Some stuff just needs to be handled for the user. These are case sensitive
            foreach (var wordMap in Constants.INTERNAL_WORD_MAPS)
            {
                if(wordMap.OnlyIfInFirstPositon && textPosition != 0)
                {
                    continue;
                }
                //dont run an internal word map if the user has one for the same thing
                if(!userMaps.Any(a=>a.Word == wordMap.Word))
                {
                    var wordToLookFor = string.Format(_wordMatchRegexTemplate, wordMap.Word);
                    converted = Regex.Replace(converted, wordToLookFor, wordMap.GetTranslation());
                }
            }
            return converted;
        }

        /// <summary> Translates text replacing words from the WordMap settings </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        internal static string TranslateText(string text)
        {
            var converted = text;
            if (_optionsService.WordMaps == null)
            {
                return converted;
            }

            var mergedWorkMaps = new HashSet<WordMap>(_optionsService.WordMaps);
            foreach (var wordMap in mergedWorkMaps)
            {
                var wordToLookFor = string.Format(_wordMatchRegexTemplate, wordMap.Word);
                converted = Regex.Replace(converted, wordToLookFor, wordMap.GetTranslation());
            }
            return converted;
        }

        private static readonly string _wordMatchRegexTemplate = @"\b({0})\b";
        private static IOptionsService _optionsService;
    }
}
