using System.Collections.Generic;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public interface IOptionPageGrid
    {
        bool ExcludeAsyncSuffix { get; set; }

        bool IncludeValueNodeInProperties { get; set; }

        bool IsEnabledForPublishMembersOnly { get; set; }

        bool UseNaturalLanguageForReturnNode { get; set; }

        bool UseToDoCommentsOnSummaryError { get; set; }

        List<WordMap> WordMaps { get; set; }
    }
}
