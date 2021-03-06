using System;
using System.Text.RegularExpressions;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor.Helper
{
    public static class Translator
    {
        private static string wordMatchRegexTemplate = @"\b({0})\b";

        /// <summary>
        ///   Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string Translate(this CSharpSyntaxNode node)
        {
            return TranslateText(node.ToString());
        }

        /// <summary>
        ///   Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string Translate(this string text)
        {
            return TranslateText(text);
        }

        /// <summary>
        ///   Translates a span of text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string Translate(this ReadOnlySpan<char> text)
        {
            return TranslateText(text).ToString();
        }

        /// <summary>
        ///   Translates a span of text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A ReadOnlySpan of char </returns>
        public static ReadOnlySpan<char> TranslateToSpan(this ReadOnlySpan<char> text)
        {
            return TranslateText(text);
        }

        /// <summary>
        ///   Translates a span of text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A ReadOnlySpan of char </returns>
        public static ReadOnlySpan<char> TranslateText(ReadOnlySpan<char> text)
        {
            if (CodeDocumentorPackage.Options?.WordMaps == null)
            {
                return text;
            }
            string converted = text.ToString();
            return TranslateText(converted).AsSpan();
        }

        /// <summary>
        ///   Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> A string </returns>
        public static string TranslateText(string text)
        {
            if (CodeDocumentorPackage.Options?.WordMaps == null)
            {
                return text;
            }
            string converted = text;

            foreach (var wordMap in CodeDocumentorPackage.Options.WordMaps)
            {
                converted = Regex.Replace(converted, string.Format(wordMatchRegexTemplate, wordMap.Word), wordMap.Translation);
            }
            return converted;
        }
    }
}
