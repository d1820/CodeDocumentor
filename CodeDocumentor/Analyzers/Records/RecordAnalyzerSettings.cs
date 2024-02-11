using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal static class RecordAnalyzerSettings
    {
        /// <summary>
        ///  The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.CATEGORY;

        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.RECORD_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        internal const string Title = "The record must have a documentation header.";

        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            return new DiagnosticDescriptor(RecordAnalyzerSettings.DiagnosticId, RecordAnalyzerSettings.Title,
                RecordAnalyzerSettings.MessageFormat, RecordAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : optionsService.RecordDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity, true);
        }
    }
}
