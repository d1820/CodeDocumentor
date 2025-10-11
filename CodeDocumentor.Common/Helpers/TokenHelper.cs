using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeDocumentor.Common.Helpers
{
    //This takes XML nodes in a string and swaps them to tokens for string manipulation, and then replaces them once complete. This keeps the validity of the XML
    public static class TokenHelper
    {
        private static readonly Regex _xmlElementRegEx = new Regex(Constants.XML_ELEMENT_MATCH_REGEX_TEMPLATE);

        public static void SwapXmlTokens(this List<string> parts, Func<string, string> swapLineCallback, int startingIndex = 0)
        {
            var i = startingIndex;
            var swaps = new Dictionary<string, string>();
            for (; i < parts.Count; i++)
            {
                var part = parts[i];
                var xmls = _xmlElementRegEx.Matches(part);
                for (var j = 0; j < xmls.Count; j++)
                {
                    var xml = xmls[j];
                    var key = $"{{xml{j}}}";
                    swaps.Add(key, xml.Value);
                    part = part.Replace(xml.Value, key);
                }
                part = swapLineCallback.Invoke(part);
                foreach (var kv in swaps)
                {
                    part = part.Replace(kv.Key, kv.Value);
                }
                parts[i] = part;
            }
        }

        public static string SwapXmlTokens(this string content, Func<string, string> swapLineCallback, int startingIndex = 0)
        {
            var i = startingIndex;
            var swaps = new Dictionary<string, string>();
            var xmls = _xmlElementRegEx.Matches(content);
            for (var j = 0; j < xmls.Count; j++)
            {
                var xml = xmls[j];
                var key = $"{{xml{j}}}";
                swaps.Add(key, xml.Value);
                content = content.Replace(xml.Value, key);
            }
            content = swapLineCallback.Invoke(content);
            foreach (var kv in swaps)
            {
                content = content.Replace(kv.Key, kv.Value);
            }
            return content;
        }

        public static (string replacedString, Dictionary<string, string> tokens) SwapXmlTokens(this string content, int startingIndex = 0)
        {
            var i = startingIndex;
            var swaps = new Dictionary<string, string>();
            var xmls = _xmlElementRegEx.Matches(content);
            for (var j = 0; j < xmls.Count; j++)
            {
                var xml = xmls[j];
                var key = $"{{xml{j}}}";
                swaps.Add(key, xml.Value);
                content = content.Replace(xml.Value, key);
            }
            return (content, swaps);
        }
    }
}
