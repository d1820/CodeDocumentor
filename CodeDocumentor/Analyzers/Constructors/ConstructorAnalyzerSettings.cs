using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class ConstructorAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The constructor must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.CONSTRUCTOR_DIAGNOSTIC_ID;

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            return new DiagnosticDescriptor(ConstructorAnalyzerSettings.DiagnosticId, ConstructorAnalyzerSettings.Title,
                ConstructorAnalyzerSettings.MessageFormat, ConstructorAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : optionsService.DefaultDiagnosticSeverity, true);
        }
    }
}
