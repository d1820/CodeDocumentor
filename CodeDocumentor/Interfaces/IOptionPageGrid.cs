using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public interface IOptionPageGrid
    {
        /// <summary> Gets or Sets a value indicating the default severity for the code analyzer. </summary>
        /// <value> A DiagnosticSeverity. </value>
        DiagnosticSeverity DefaultDiagnosticSeverity { get; set; }

        /// <summary> Gets or Sets a value indicating whether exclude asynchronously suffix. </summary>
        /// <value> A bool. </value>
        bool ExcludeAsyncSuffix { get; set; }

        /// <summary> Gets or Sets a value indicating whether include value node in properties. </summary>
        /// <value> A bool. </value>
        bool IncludeValueNodeInProperties { get; set; }

        /// <summary> Gets or Sets a value indicating whether enabled for public members is only. </summary>
        /// <value> A bool. </value>
        bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary> Gets or Sets a value indicating whether preserve existing summary text. </summary>
        bool PreserveExistingSummaryText { get; set; }

        /// <summary> Gets or Sets a value indicating whether use natural language for return node. </summary>
        /// <value> A bool. </value>
        bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary> Gets or Sets a value indicating whether use to do comments on summary error. </summary>
        /// <value> A bool. </value>
        bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary> Gets or Sets the word maps. </summary>
        /// <value> A list of wordmaps. </value>
        WordMap[] WordMaps { get; set; }
    }
}
