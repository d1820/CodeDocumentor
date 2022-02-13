using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class FieldAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The field must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = "CD1603";

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///   The diagnostic descriptor rule.
        /// </summary>
        internal static DiagnosticDescriptor GetRule()
        {
            return new DiagnosticDescriptor(FieldAnalyzerSettings.DiagnosticId, FieldAnalyzerSettings.Title, FieldAnalyzerSettings.MessageFormat, 
                FieldAnalyzerSettings.Category, CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning, true);
        }
    }
}
