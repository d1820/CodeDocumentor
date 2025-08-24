using CodeDocumentor.Analyzers;
using CodeDocumentor.Common;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class EnumAnalyzerSettings : BaseAnalyzerSettings
    {
        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        internal const string Title = "The enum must have a documentation header.";

        /// <summary>
        ///  The diagnostic descriptor rule.
        /// </summary>
        internal DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            return new DiagnosticDescriptor(DiagnosticId, Title,
                MessageFormat, Category,
                hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId), true);
        }
    }
}
