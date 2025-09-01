using System;
using System.Linq;
using System.Threading.Tasks;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Helper
{
    public static class EditorConfigExtensions
    {
        public static async Task<ISettings> BuildSettingsAsync(this CodeFixContext context, ISettings staticSettings)
        {
            var tree = await context.Document.GetSyntaxTreeAsync();
            return context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree).BuildSettings(staticSettings);
        }
        public static ISettings BuildSettings(this SyntaxNodeAnalysisContext context, ISettings staticSettings)
        {
            return context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree).BuildSettings(staticSettings);
        }
        public static ISettings BuildSettings(this AnalyzerConfigOptions options, ISettings staticSettings)
        {
            var settings = new Settings();
            var defaultSev = DiagnosticSeverity.Warning;
            if (!options.CanReadEditorConfig())
            {
                //no editorconfig, return the static settings we have
                return staticSettings;
            }

            settings.ClassDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_class_diagram_severity", defaultSev);
            settings.ConstructorDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_constructor_diagram_severity", defaultSev);
            settings.DefaultDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_default_diagram_severity", defaultSev);
            settings.EnumDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_enum_diagram_severity", defaultSev);
            settings.FieldDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_field_diagram_severity", defaultSev);
            settings.InterfaceDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_interface_diagram_severity", defaultSev);
            settings.MethodDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_method_diagram_severity", defaultSev);
            settings.PropertyDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_property_diagram_severity", defaultSev);
            settings.RecordDiagnosticSeverity = options.ConvertToDiagnosticSeverity("codedocumentor_record_diagram_severity", defaultSev);

            settings.ExcludeAsyncSuffix = options.ConvertToBoolean("codedocumentor_exclude_async_suffix", false);

            settings.IncludeValueNodeInProperties = options.ConvertToBoolean("codedocumentor_include_value_node_in_properties", false);
            settings.IsEnabledForPublicMembersOnly = options.ConvertToBoolean("codedocumentor_is_enabled_for_public_members_only", false);
            settings.IsEnabledForNonPublicFields = options.ConvertToBoolean("codedocumentor_is_enabled_for_non_public_fields", false);
            settings.PreserveExistingSummaryText = options.ConvertToBoolean("codedocumentor_preserve_existing_summary_text", true);
            settings.TryToIncludeCrefsForReturnTypes = options.ConvertToBoolean("codedocumentor_try_to_include_crefs_for_return_types", true);
            settings.UseToDoCommentsOnSummaryError = options.ConvertToBoolean("codedocumentor_use_natural_language_for_return_node", false);
            settings.UseToDoCommentsOnSummaryError = options.ConvertToBoolean("codedocumentor_use_todo_comments_on_summary_error", true);
            settings.WordMaps = options.ConvertToWordMap("codedocumentor_wordmap", Constants.DEFAULT_WORD_MAPS);
            return settings;
        }

        private static bool CanReadEditorConfig(this AnalyzerConfigOptions options)
        {
            return options.Keys.Any(a => a.StartsWith("codedocumentor_"));
        }


        private static WordMap[] ConvertToWordMap(this AnalyzerConfigOptions options, string key, WordMap[] defaultWordMaps)
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

        private static bool ConvertToBoolean(this AnalyzerConfigOptions options, string key, bool defaulBool)
        {
            options.TryGetValue(key, out var cds);
            if (string.IsNullOrEmpty(cds))
            {
                return defaulBool;
            }
            if (bool.TryParse(cds, out var converted))
            {
                return converted;
            }
            return defaulBool;
        }

        private static DiagnosticSeverity ConvertToDiagnosticSeverity(this AnalyzerConfigOptions options, string key, DiagnosticSeverity defaultSeverity)
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
    }
}
