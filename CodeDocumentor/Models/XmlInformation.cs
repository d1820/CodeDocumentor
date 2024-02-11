// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System.Text.RegularExpressions;

namespace CodeDocumentor.Vsix2022
{
    public class XmlInformation
    {
        //bool isXml, bool isGeneric, bool isTypeParam

        public XmlInformation(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            IsXml = Regex.IsMatch(text, "CDATA");
            IsGeneric = Regex.IsMatch(text, @"(\w+\<)");
            IsTypeParam = Regex.IsMatch(text, "(<typeparam)");
            IsSeeNode = Regex.IsMatch(text, "(<see)");
        }

        public bool IsXml { get; }
        public bool IsGeneric { get; }
        public bool IsTypeParam { get; }
        public bool IsSeeNode { get; }
    }
}
