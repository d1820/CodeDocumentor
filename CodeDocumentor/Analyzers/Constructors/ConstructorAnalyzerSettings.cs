using CodeDocumentor.Analyzers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class ConstructorAnalyzerSettings: BaseAnalyzerSettings
    {
        /// <summary>
        ///  The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.CONSTRUCTOR_DIAGNOSTIC_ID;

        /// <summary>
        ///  The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///  The title.
        /// </summary>
        internal const string Title = "The constructor must have a documentation header.";

        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            return new DiagnosticDescriptor(ConstructorAnalyzerSettings.DiagnosticId, ConstructorAnalyzerSettings.Title,
                ConstructorAnalyzerSettings.MessageFormat, ConstructorAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : LookupSeverity(DiagnosticId), true);
        }
    }
}
