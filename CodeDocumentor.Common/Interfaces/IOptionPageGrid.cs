using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Common.Interfaces
{
    public interface IOptionPageGrid
    {
        DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating the default severity for the code analyzer.
        /// </summary>
        /// <value> A DiagnosticSeverity. </value>
        DiagnosticSeverity DefaultDiagnosticSeverity { get; set; }

        DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        bool ExcludeAsyncSuffix { get; set; }

        DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        bool IncludeValueNodeInProperties { get; set; }

        DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        bool IsEnabledForNonPublicFields { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for public members is only.
        /// </summary>
        /// <value> A bool. </value>
        bool IsEnabledForPublicMembersOnly { get; set; }

        DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        bool PreserveExistingSummaryText { get; set; }

        DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use try and include return type crefs in documentation.
        /// </summary>
        /// <value> A bool. </value>
        bool TryToIncludeCrefsForReturnTypes { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether to use the .editorconfig file for settings.
        /// </summary>
        /// <remarks> This will convert the existing settings to a %USERPROFILE% .editorconfig file </remarks>
        bool UseEditorConfigForSettings { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use natural language for return node.
        /// </summary>
        /// <value> A bool. </value>
        bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use to do comments on summary error.
        /// </summary>
        /// <value> A bool. </value>
        bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary>
        ///  Gets or Sets the word maps.
        /// </summary>
        /// <value> A list of wordmaps. </value>
        WordMap[] WordMaps { get; set; }
    }
}
