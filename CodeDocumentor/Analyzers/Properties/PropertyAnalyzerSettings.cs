using CodeDocumentor.Analyzers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class PropertyAnalyzerSettings: BaseAnalyzerSettings
    {
        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.PROPERTY_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        internal const string Title = "The property must have a documentation header.";

        /// <summary>
        ///  The diagnostic descriptor rule.
        /// </summary>
        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            return new DiagnosticDescriptor(PropertyAnalyzerSettings.DiagnosticId, PropertyAnalyzerSettings.Title,
                PropertyAnalyzerSettings.MessageFormat, PropertyAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId), true);
        }
    }
}
