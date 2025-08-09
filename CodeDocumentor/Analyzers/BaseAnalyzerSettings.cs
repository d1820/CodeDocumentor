using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Analyzers
{
    internal class BaseAnalyzerSettings
    {
        /// <summary>
        ///  The category.
        /// </summary>
        internal const string Category = Constants.CATEGORY;

        protected static IOptionsService OptionsService;

        public static void SetOptionsService(IOptionsService optionsService)
        {
            OptionsService = optionsService;
        }


        internal static DiagnosticSeverity LookupSeverity(string diagnosticId)
        {
            if (OptionsService == null)
            {
                return Constants.DefaultDiagnosticSeverityOnError;
            }
            return TryHelper.Try(() =>
            {
                var optionsService = OptionsService;
                switch (diagnosticId)
                {
                    case Constants.DiagnosticIds.CLASS_DIAGNOSTIC_ID:
                        return optionsService.ClassDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.CONSTRUCTOR_DIAGNOSTIC_ID:
                        return optionsService.ConstructorDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.INTERFACE_DIAGNOSTIC_ID:
                        return optionsService.InterfaceDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID:
                        return optionsService.EnumDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.FIELD_DIAGNOSTIC_ID:
                        return optionsService.FieldDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.METHOD_DIAGNOSTIC_ID:
                        return optionsService.MethodDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.PROPERTY_DIAGNOSTIC_ID:
                        return optionsService.PropertyDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.RECORD_DIAGNOSTIC_ID:
                        return optionsService.RecordDiagnosticSeverity ?? optionsService.DefaultDiagnosticSeverity;
                }
                return Constants.DefaultDiagnosticSeverityOnError;
            }, diagnosticId, (_) => Constants.DefaultDiagnosticSeverityOnError, eventId: Constants.EventIds.ANALYZER);
        }
    }
}
