using CodeDocumentor.Analyzers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class FieldAnalyzerSettings:BaseAnalyzerSettings
    {
        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.FIELD_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        internal const string Title = "The field must have a documentation header.";

        /// <summary>
        ///  The diagnostic descriptor rule.
        /// </summary>
        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            return new DiagnosticDescriptor(FieldAnalyzerSettings.DiagnosticId, FieldAnalyzerSettings.Title, FieldAnalyzerSettings.MessageFormat,
                FieldAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId), true);
        }
    }
}
