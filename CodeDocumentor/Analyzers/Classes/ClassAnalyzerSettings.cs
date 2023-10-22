using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal class ClassAnalyzerSettings
    {
        /// <summary> The category. </summary>
        internal const string Category = DocumentationHeaderHelper.CATEGORY;

        /// <summary> The diagnostic id. </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.CLASS_DIAGNOSTIC_ID;

        /// <summary> The message format. </summary>
        internal const string MessageFormat = Title;

        /// <summary> The title. </summary>
        internal const string Title = "The class must have a documentation header.";

        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            return new DiagnosticDescriptor(ClassAnalyzerSettings.DiagnosticId, ClassAnalyzerSettings.Title,
                ClassAnalyzerSettings.MessageFormat, ClassAnalyzerSettings.Category,
                 hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : optionsService.DefaultDiagnosticSeverity, true);
        }
    }
}
