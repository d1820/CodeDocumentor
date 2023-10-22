using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Services
{
    public interface IOptionsService : IOptionPageGrid
    {
        void Update(Vsix2022.Settings settings);
    }

    public class OptionsService : IOptionsService
    {
        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning;

        public bool ExcludeAsyncSuffix { get; set; } = CodeDocumentorPackage.Options?.ExcludeAsyncSuffix ?? false;

        public bool IncludeValueNodeInProperties { get; set; } = CodeDocumentorPackage.Options?.IncludeValueNodeInProperties ?? false;

        public bool IsEnabledForPublicMembersOnly { get; set; } = CodeDocumentorPackage.Options?.IsEnabledForPublicMembersOnly ?? false;

        public bool PreserveExistingSummaryText { get; set; } = CodeDocumentorPackage.Options?.PreserveExistingSummaryText ?? true;

        public bool UseNaturalLanguageForReturnNode { get; set; } = CodeDocumentorPackage.Options?.UseNaturalLanguageForReturnNode ?? false;

        public bool UseToDoCommentsOnSummaryError { get; set; } = CodeDocumentorPackage.Options?.UseToDoCommentsOnSummaryError ?? false;

        public WordMap[] WordMaps { get; set; } = CodeDocumentorPackage.Options?.WordMaps ?? Constants.WORD_MAPS;

        public void Update(Vsix2022.Settings settings)
        {
            IsEnabledForPublicMembersOnly = settings.IsEnabledForPublicMembersOnly;
            UseNaturalLanguageForReturnNode = settings.UseNaturalLanguageForReturnNode;
            ExcludeAsyncSuffix = settings.ExcludeAsyncSuffix;
            IncludeValueNodeInProperties = settings.IncludeValueNodeInProperties;
            UseToDoCommentsOnSummaryError = settings.UseToDoCommentsOnSummaryError;
            WordMaps = settings.WordMaps;
            DefaultDiagnosticSeverity = settings.DefaultDiagnosticSeverity;
            PreserveExistingSummaryText = settings.PreserveExistingSummaryText;
        }
    }
}
