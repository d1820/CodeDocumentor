using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class ClassAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The class must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = "CD1600";

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        internal static DiagnosticDescriptor GetRule()
        {
            return new DiagnosticDescriptor(ClassAnalyzerSettings.DiagnosticId, ClassAnalyzerSettings.Title, 
                ClassAnalyzerSettings.MessageFormat, ClassAnalyzerSettings.Category, 
                CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning, true);
        }

    }
}
