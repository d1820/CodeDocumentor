using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Analyzers.Analyzers.Fields
{
    public class FieldAnalyzerSettings : BaseAnalyzerSettings
    {
        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        public const string DiagnosticId = Constants.DiagnosticIds.FIELD_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        public const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        public const string Title = "The field must have a documentation header.";

        public DiagnosticDescriptor GetSupportedDiagnosticRule()
        {
            return new DiagnosticDescriptor(DiagnosticId, Title,
                MessageFormat, Category,
                 DiagnosticSeverity.Info,
                 true);
        }

        /// <summary>
        ///  The diagnostic descriptor rule.
        /// </summary>
        public DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity, ISettings settings)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
                Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId, settings), true);
        }
    }
}
