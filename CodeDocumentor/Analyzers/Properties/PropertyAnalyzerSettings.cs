using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal static class PropertyAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The property must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = "CD1606";

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///   The diagnostic descriptor rule.
        /// </summary>
        internal static DiagnosticDescriptor GetRule()
        {
            return new DiagnosticDescriptor(PropertyAnalyzerSettings.DiagnosticId, PropertyAnalyzerSettings.Title, 
                PropertyAnalyzerSettings.MessageFormat, PropertyAnalyzerSettings.Category, 
                CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning, true);
        }
    }
}
