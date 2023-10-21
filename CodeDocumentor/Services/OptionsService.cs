
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Services
{
    public class OptionsService : IOptionsService
    {
        public bool ExcludeAsyncSuffix { get; set; } = CodeDocumentorPackage.Options?.ExcludeAsyncSuffix ?? false;
        public bool IncludeValueNodeInProperties { get; set; } = CodeDocumentorPackage.Options?.IncludeValueNodeInProperties ?? false;
        public bool IsEnabledForPublicMembersOnly { get; set; } = CodeDocumentorPackage.Options?.IsEnabledForPublicMembersOnly ?? false;
        public bool UseNaturalLanguageForReturnNode { get; set; } = CodeDocumentorPackage.Options?.UseNaturalLanguageForReturnNode ?? false;
        public bool UseToDoCommentsOnSummaryError { get; set; } = CodeDocumentorPackage.Options?.UseToDoCommentsOnSummaryError ?? false;
        public bool PreserveExistingSummaryText { get; set; } = CodeDocumentorPackage.Options?.PreserveExistingSummaryText ?? true;
        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning;
        public WordMap[] WordMaps { get; set; } = CodeDocumentorPackage.Options?.WordMaps ?? Constants.WORD_MAPS;
    }

    public interface IOptionsService : IOptionPageGrid
    {
    }
}
