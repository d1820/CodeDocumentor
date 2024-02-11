using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Test.TestHelpers
{
    [SuppressMessage("XMLDocumentation", "")]
    public class TestOptionsService : IOptionsService
    {
        public bool ExcludeAsyncSuffix { get; set; }

        public bool IncludeValueNodeInProperties { get; set; }

        public bool IsEnabledForPublicMembersOnly { get; set; }

        public bool UseNaturalLanguageForReturnNode { get; set; }

        public bool UseToDoCommentsOnSummaryError { get; set; }

        public bool TryToIncludeCrefsForReturnTypes { get; set; }

        public WordMap[] WordMaps { get; set; } = Constants.DEFAULT_WORD_MAPS;

        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = DiagnosticSeverity.Warning;

        public bool PreserveExistingSummaryText { get; set; }
        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }
        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }
        public bool IsEnabledForNonPublicFields { get; set; }

        public void SetDefaults(IOptionPageGrid options)
        {

        }

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
            ClassDiagnosticSeverity = settings.ClassDiagnosticSeverity;
            ConstructorDiagnosticSeverity = settings.ConstructorDiagnosticSeverity;
            EnumDiagnosticSeverity = settings.EnumDiagnosticSeverity;
            FieldDiagnosticSeverity = settings.FieldDiagnosticSeverity;
            InterfaceDiagnosticSeverity = settings.InterfaceDiagnosticSeverity;
            MethodDiagnosticSeverity = settings.MethodDiagnosticSeverity;
            PropertyDiagnosticSeverity = settings.PropertyDiagnosticSeverity;
            RecordDiagnosticSeverity = settings.RecordDiagnosticSeverity;
        }
    }
}
