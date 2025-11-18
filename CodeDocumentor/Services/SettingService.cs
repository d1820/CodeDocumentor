using System;
using System.Linq;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Common.Services
{
    public class SettingService : ISettingService
    {
        private readonly IEventLogger _eventLogger;
        private ISettings _staticSettings = Settings.BuildDefaults();

        public SettingService(IEventLogger eventLogger)
        {
            _eventLogger = eventLogger;
        }

        public ISettings StaticSettings
        {
            get => _staticSettings.Clone();
            set => _staticSettings = value;
        }
        public ISettings BuildSettings(SyntaxNodeAnalysisContext context)
        {
            var opts = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            return BuildSettings(opts);
        }

        public ISettings BuildSettings(AnalyzerConfigOptions options)
        {
            var settings = new Settings();
            var defaultSev = DiagnosticSeverity.Warning;
            if (!CanReadEditorConfig(options))
            {
                _eventLogger.LogDebug(Constants.CATEGORY, $"{nameof(BuildSettings)}: CanReadEditorConfig == false");
                //no editorconfig, return the  settings we have
                return StaticSettings;
            }
            _eventLogger.LogDebug(Constants.CATEGORY, $"{nameof(BuildSettings)}: CanReadEditorConfig == true");
            settings.ClassDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_class_diagram_severity", defaultSev);
            settings.ConstructorDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_constructor_diagram_severity", defaultSev);
            settings.DefaultDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_default_diagram_severity", defaultSev);
            settings.EnumDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_enum_diagram_severity", defaultSev);
            settings.FieldDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_field_diagram_severity", defaultSev);
            settings.InterfaceDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_interface_diagram_severity", defaultSev);
            settings.MethodDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_method_diagram_severity", defaultSev);
            settings.PropertyDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_property_diagram_severity", defaultSev);
            settings.RecordDiagnosticSeverity = ConvertToDiagnosticSeverity(options, "codedocumentor_record_diagram_severity", defaultSev);

            settings.ExcludeAsyncSuffix = ConvertToBoolean(options, "codedocumentor_exclude_async_suffix", false);

            settings.IncludeValueNodeInProperties = ConvertToBoolean(options, "codedocumentor_include_value_node_in_properties", false);
            settings.IsEnabledForPublicMembersOnly = ConvertToBoolean(options, "codedocumentor_is_enabled_for_public_members_only", false);
            settings.IsEnabledForNonPublicFields = ConvertToBoolean(options, "codedocumentor_is_enabled_for_non_public_fields", false);
            settings.PreserveExistingSummaryText = ConvertToBoolean(options, "codedocumentor_preserve_existing_summary_text", true);
            settings.TryToIncludeCrefsForReturnTypes = ConvertToBoolean(options, "codedocumentor_try_to_include_crefs_for_return_types", true);
            settings.UseNaturalLanguageForReturnNode = ConvertToBoolean(options, "codedocumentor_use_natural_language_for_return_node", false);
            settings.UseToDoCommentsOnSummaryError = ConvertToBoolean(options, "codedocumentor_use_todo_comments_on_summary_error", true);
            settings.WordMaps = ConvertToWordMap(options, "codedocumentor_wordmap", Constants.DEFAULT_WORD_MAPS);
            return settings;
        }

        private bool CanReadEditorConfig(AnalyzerConfigOptions options)
        {
            return options?.Keys.Any(a => a.StartsWith("codedocumentor_")) ?? false;
        }

        private bool ConvertToBoolean(AnalyzerConfigOptions options, string key, bool defaultBool)
        {
            options.TryGetValue(key, out var cds);
            if (string.IsNullOrEmpty(cds))
            {
                return defaultBool;
            }
            if (bool.TryParse(cds, out var converted))
            {
                return converted;
            }
            return defaultBool;
        }

        private DiagnosticSeverity ConvertToDiagnosticSeverity(AnalyzerConfigOptions options, string key, DiagnosticSeverity defaultSeverity)
        {
            options.TryGetValue(key, out var cds);
            if (string.IsNullOrEmpty(cds))
            {
                return defaultSeverity;
            }
            if (Enum.TryParse<DiagnosticSeverity>(cds, out var converted))
            {
                return converted;
            }
            return defaultSeverity;
        }

        private WordMap[] ConvertToWordMap(AnalyzerConfigOptions options, string key, WordMap[] defaultWordMaps)
        {
            options.TryGetValue(key, out var cds);
            if (string.IsNullOrWhiteSpace(cds))
            {
                return defaultWordMaps;
            }

            return cds
                .Split('|')
                .Select(pair => pair.Split(':'))
                .Where(parts => parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                .Select(parts => new WordMap
                {
                    Word = parts[0],
                    Translation = parts[1]
                })
                .ToArray();
        }
    }
}
