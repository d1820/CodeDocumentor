// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System.Text.RegularExpressions;

namespace CodeDocumentor.Vsix2022
{
    public class XmlInformation
    {
        public string CDataMatch { get; }

        public string GenericMatch { get; }

        public bool HasSeeCrefNode { get; }

        public bool HasText { get; }

        public bool HasTypeParam { get; }

        public bool IsCData { get; }

        public bool IsGeneric { get; }

        public string SeeCrefMatch { get; }

        public string TypeParamMatch { get; }

        public XmlInformation(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var match = Regex.Match(text, @"<!\[CDATA\[.*\]\]>");
            IsCData = match.Success;
            CDataMatch = match?.Value;

            match = Regex.Match(text, @"(^\w+<.*>$)");
            IsGeneric = match.Success;
            GenericMatch = match?.Value;

            match = Regex.Match(text, @"(<typeparamref.*?>)");
            HasTypeParam = match.Success;
            TypeParamMatch = match?.Value;

            match = Regex.Match(text, @"(<see.*?>)");
            HasSeeCrefNode = match.Success;
            SeeCrefMatch = match?.Value;

            HasText = Regex.IsMatch(text, @"^\w*|\w*$");
        }
    }
}
