using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class FileAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The file needs documentation headers.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.CATEGORY;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.FILE_DIAGNOSTIC_ID;

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        internal static DiagnosticDescriptor GetRule()
        {
            //we dont need to show this to still show the option to decorate the whole file. Setting DiagnosticSeverity.Hidden
            return new DiagnosticDescriptor(FileAnalyzerSettings.DiagnosticId, FileAnalyzerSettings.Title,
                FileAnalyzerSettings.MessageFormat, FileAnalyzerSettings.Category, DiagnosticSeverity.Hidden, true);
        }
    }
}
