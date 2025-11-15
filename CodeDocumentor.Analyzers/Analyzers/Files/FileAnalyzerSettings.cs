using CodeDocumentor.Common;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Analyzers
{
    public class FileAnalyzerSettings : BaseAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        public const string Title = "The file needs documentation headers.";

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        public const string DiagnosticId = Constants.DiagnosticIds.FILE_DIAGNOSTIC_ID;

        /// <summary>
        ///   The message format.
        /// </summary>
        public const string MessageFormat = Title;

        public static DiagnosticDescriptor GetRule()
        {
            //we dont need to show this to still show the option to decorate the whole file. Setting DiagnosticSeverity.Hidden
            return new DiagnosticDescriptor(DiagnosticId, Title,
                MessageFormat, Category, DiagnosticSeverity.Hidden, true);
        }
    }
}
