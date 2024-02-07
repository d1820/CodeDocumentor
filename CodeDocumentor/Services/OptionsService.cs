using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Services
{
    public interface IOptionsService : IOptionPageGrid
    {
        void Update(Vsix2022.Settings settings);

        void SetDefaults(IOptionPageGrid options);
    }

    public class OptionsService : IOptionsService
    {
        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        public bool ExcludeAsyncSuffix { get; set; }

        public bool IncludeValueNodeInProperties { get; set; }

        public bool IsEnabledForPublicMembersOnly { get; set; }

        public bool IsEnabledForNonPublicFields { get; set; }

        public bool PreserveExistingSummaryText { get; set; }

        public bool UseNaturalLanguageForReturnNode { get; set; }

        public bool UseToDoCommentsOnSummaryError { get; set; }

        public WordMap[] WordMaps { get; set; }

        public void SetDefaults(IOptionPageGrid options)
        {
            DefaultDiagnosticSeverity = options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning;

            ClassDiagnosticSeverity = options.ClassDiagnosticSeverity;

            ConstructorDiagnosticSeverity = options.ConstructorDiagnosticSeverity;

            EnumDiagnosticSeverity = options.EnumDiagnosticSeverity;

            FieldDiagnosticSeverity = options.FieldDiagnosticSeverity;

            InterfaceDiagnosticSeverity = options.InterfaceDiagnosticSeverity;

            MethodDiagnosticSeverity = options.MethodDiagnosticSeverity;

            PropertyDiagnosticSeverity = options.PropertyDiagnosticSeverity;

            RecordDiagnosticSeverity = options.RecordDiagnosticSeverity;

            ExcludeAsyncSuffix = options?.ExcludeAsyncSuffix ?? false;

            IncludeValueNodeInProperties = options?.IncludeValueNodeInProperties ?? false;

            IsEnabledForPublicMembersOnly = options?.IsEnabledForPublicMembersOnly ?? false;

            IsEnabledForNonPublicFields = options?.IsEnabledForNonPublicFields ?? false;

            PreserveExistingSummaryText = options?.PreserveExistingSummaryText ?? true;

            UseNaturalLanguageForReturnNode = options?.UseNaturalLanguageForReturnNode ?? false;

            UseToDoCommentsOnSummaryError = options?.UseToDoCommentsOnSummaryError ?? false;

            WordMaps = options?.WordMaps ?? Constants.WORD_MAPS;
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
            IsEnabledForNonPublicFields = settings.IsEnabledForNonPublicFields;
        }
    }
}
