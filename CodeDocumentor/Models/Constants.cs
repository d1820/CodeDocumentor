using System.Collections.Generic;
// For definitions of XML nodes see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
// see also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public static class Constants
    {
        public static List<WordMap> WORD_MAPS = new List<WordMap> {
            new WordMap { Word = "int", Translation = "integer" },
            new WordMap { Word = "Int32", Translation = "integer" },
            new WordMap { Word = "Int64", Translation = "integer" },
            new WordMap { Word = "OfList", Translation = "OfLists" },
            new WordMap { Word = "OfCollection", Translation = "OfCollections" },
            new WordMap { Word = "OfEnumerable", Translation = "OfLists" },
            new WordMap { Word = "IEnumerable", Translation = "List" },
            new WordMap { Word = "ICollection", Translation = "Collection" },
            new WordMap { Word = "IReadOnlyCollection", Translation = "Read Only Collection" },
            new WordMap { Word = "IList", Translation = "List" },
            new WordMap { Word = "IReadOnlyDictionary", Translation = "Read Only Dictionary" },
            new WordMap { Word = "IReadOnlyList", Translation = "Read Only List" },

        };
    }

}
