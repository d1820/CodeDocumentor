using CodeDocumentor.Analyzers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class RecordAnalyzerSettings: BaseAnalyzerSettings
    {
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
            return new DiagnosticDescriptor(DiagnosticId, Title,
                MessageFormat, Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId), true);
        }
    }
}
