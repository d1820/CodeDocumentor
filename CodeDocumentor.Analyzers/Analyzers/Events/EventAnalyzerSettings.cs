using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Analyzers.Analyzers.Events
{
    public class EventAnalyzerSettings : BaseAnalyzerSettings
    {
        public const string DiagnosticId = Constants.DiagnosticIds.EVENT_DIAGNOSTIC_ID;
        public const string MessageFormat = Title;
        public const string Title = "The event must have a documentation header.";

        public DiagnosticDescriptor GetSupportedDiagnosticRule()
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, true);
        }

        public DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity, ISettings settings)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId, settings), true);
        }
    }
}
