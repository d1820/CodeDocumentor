// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System.Text.RegularExpressions;

namespace CodeDocumentor.Common.Models
{
    public class XmlInformation
    {
        public bool HasSeeCrefNode { get; }

        public bool HasText { get; }

        public bool HasTypeParam { get; }

        public bool IsCData { get; }

        public bool IsGeneric { get; }

        public XmlInformation(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var match = Regex.Match(text, @"<!\[CDATA\[.*\]\]>");
            IsCData = match.Success;

            match = Regex.Match(text, @"(^\w+<.*>$)");
            IsGeneric = match.Success;

            match = Regex.Match(text, "(<typeparamref.*?>)");
            HasTypeParam = match.Success;

            match = Regex.Match(text, "(<see.*?>)");
            HasSeeCrefNode = match.Success;

            HasText = !string.IsNullOrEmpty(Regex.Match(text, @"^\w*|\w*$")?.Value.Trim());
        }
    }
}
