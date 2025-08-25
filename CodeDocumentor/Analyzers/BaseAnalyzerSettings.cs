using System;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Locators;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Analyzers
{
    internal class BaseAnalyzerSettings
    {
        /// <summary>
        ///  The category.
        /// </summary>
        internal const string Category = Constants.CATEGORY;

        private static ISettings _settings;

        protected IEventLogger EventLogger = ServiceLocator.Logger;

        public static void SetSettings(ISettings settings)
        {
            _settings = settings;
        }

        protected static ISettings Settings =>
              //we serve up a fresh new instance from the static, and use that instead, keeps everything testable and decoupled from the static
              _settings.Clone();


        internal DiagnosticSeverity LookupSeverity(string diagnosticId)
        {
            if (Settings == null)
            {
                return Constants.DefaultDiagnosticSeverityOnError;
            }
            return TryHelper.Try(() =>
            {
                var settings = Settings;
                switch (diagnosticId)
                {
                    case Constants.DiagnosticIds.CLASS_DIAGNOSTIC_ID:
                        return settings.ClassDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.CONSTRUCTOR_DIAGNOSTIC_ID:
                        return settings.ConstructorDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.INTERFACE_DIAGNOSTIC_ID:
                        return settings.InterfaceDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID:
                        return settings.EnumDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.FIELD_DIAGNOSTIC_ID:
                        return settings.FieldDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.METHOD_DIAGNOSTIC_ID:
                        return settings.MethodDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.PROPERTY_DIAGNOSTIC_ID:
                        return settings.PropertyDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                    case Constants.DiagnosticIds.RECORD_DIAGNOSTIC_ID:
                        return settings.RecordDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity;
                }
                return Constants.DefaultDiagnosticSeverityOnError;
            }, diagnosticId, EventLogger, (_) => Constants.DefaultDiagnosticSeverityOnError, eventId: Constants.EventIds.ANALYZER);
        }
    }
}
