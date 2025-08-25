#pragma warning disable IDE0130
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace CodeDocumentor.Common
{
    public static class SettingsExtensions
    {
        public const string PREFIX = "codedocumentor_";
        //This impl was adopted from https://github.com/mike-ward/VSColorOutput64/tree/db549b54709ca77ae5538c4046c332f1e51f90e7

        private static readonly string _programDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeDocumentor");

        private static readonly string _userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

       

        public static bool IsCodeDocumentorDefinedInEditorConfig(this ISettings settings)
        {
            var editorConfigPath = Path.Combine(_userProfileFolder, ".editorconfig");
            if (!File.Exists(editorConfigPath))
            {
                return false;
            }
            var lines = File.ReadAllLines(editorConfigPath);
            return lines.Any(line => line.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///  Loads the <see cref="Settings"/>.
        /// </summary>
        /// <returns> A Settings. </returns>
        public static ISettings Load(this ISettings settings)
        {
            if (Runtime.RunningUnitTests)
            {
                return new Settings();
            }

            Directory.CreateDirectory(_programDataFolder);
            var json = File.ReadAllText(GetSettingsFilePath());
            settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
            return settings;
        }

        public static void Save(this ISettings settings)
        {
            if (Runtime.RunningUnitTests)
            {
                return;
            }

            Directory.CreateDirectory(_programDataFolder);
            settings.SaveToFile(GetSettingsFilePath());
        }

        public static void SaveToEditorConfig(this ISettings settings, Action<string> setToClipboardAction)
        {
            if (Runtime.RunningUnitTests)
            {
                return;
            }

            var editorConfigPath = Path.Combine(_userProfileFolder, ".editorconfig");
            var lines = File.ReadAllLines(editorConfigPath).ToList();

            var clipboardLInes = new List<string>();

            if (!lines.Any(line => line.StartsWith("[*.cs]", StringComparison.OrdinalIgnoreCase)))
            {
                clipboardLInes.Add("[*.cs]");
            }
            if (!lines.Any(line => line.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase)))
            {
                clipboardLInes.Add("# CodeDocumentor settings");
            }

            clipboardLInes.Add($"{PREFIX}class_diagram_severity = {settings.ClassDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}constructor_diagram_severity = {settings.ConstructorDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}default_diagram_severity = {settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}enum_diagram_severity = {settings.EnumDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}field_diagram_severity = {settings.FieldDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}interface_diagram_severity = {settings.InterfaceDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}method_diagram_severity = {settings.MethodDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}property_diagram_severity = {settings.PropertyDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}record_diagram_severity = {settings.RecordDiagnosticSeverity ?? settings.DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}exclude_async_suffix = {settings.ExcludeAsyncSuffix}");
            clipboardLInes.Add($"{PREFIX}include_value_node_in_properties = {settings.IncludeValueNodeInProperties}");
            clipboardLInes.Add($"{PREFIX}is_enabled_for_public_members_only = {settings.IsEnabledForPublicMembersOnly}");
            clipboardLInes.Add($"{PREFIX}is_enabled_for_non_public_fields = {settings.IsEnabledForNonPublicFields}");
            clipboardLInes.Add($"{PREFIX}preserve_existing_summary_text = {settings.PreserveExistingSummaryText}");
            clipboardLInes.Add($"{PREFIX}try_to_include_crefs_for_return_types = {settings.TryToIncludeCrefsForReturnTypes}");
            clipboardLInes.Add($"{PREFIX}use_natural_language_for_return_node = {settings.UseNaturalLanguageForReturnNode}");
            clipboardLInes.Add($"{PREFIX}use_todo_comments_on_summary_error = {settings.UseToDoCommentsOnSummaryError}");

            if (settings.WordMaps != null && settings.WordMaps.Length > 0)
            {
                var maps = settings.WordMaps.Where(wm => !string.IsNullOrWhiteSpace(wm?.Word) || !string.IsNullOrWhiteSpace(wm?.Translation))
                                    .Select(s => $"{s.Word}:{s.Translation}").ToList();
                clipboardLInes.Add($"{PREFIX}wordmap = {string.Join("|", maps)}");
            }
            setToClipboardAction.Invoke(string.Join(Environment.NewLine, clipboardLInes));
        }

        /// <summary>
        ///  Saves the settings to file.
        /// </summary>
        /// <param name="path"> The path. </param>
        public static void SaveToFile(this ISettings settings, string path)
        {
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(settings));
        }

        public static void SetFromOptionsGrid(this ISettings settings, ISettings optionsGrid)
        {
            settings.DefaultDiagnosticSeverity = optionsGrid?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning;
            settings.ClassDiagnosticSeverity = optionsGrid.ClassDiagnosticSeverity;
            settings.ConstructorDiagnosticSeverity = optionsGrid.ConstructorDiagnosticSeverity;
            settings.EnumDiagnosticSeverity = optionsGrid.EnumDiagnosticSeverity;
            settings.FieldDiagnosticSeverity = optionsGrid.FieldDiagnosticSeverity;
            settings.InterfaceDiagnosticSeverity = optionsGrid.InterfaceDiagnosticSeverity;
            settings.MethodDiagnosticSeverity = optionsGrid.MethodDiagnosticSeverity;
            settings.PropertyDiagnosticSeverity = optionsGrid.PropertyDiagnosticSeverity;
            settings.RecordDiagnosticSeverity = optionsGrid.RecordDiagnosticSeverity;
            settings.ExcludeAsyncSuffix = optionsGrid?.ExcludeAsyncSuffix ?? false;
            settings.IncludeValueNodeInProperties = optionsGrid?.IncludeValueNodeInProperties ?? false;
            settings.IsEnabledForPublicMembersOnly = optionsGrid?.IsEnabledForPublicMembersOnly ?? false;
            settings.IsEnabledForNonPublicFields = optionsGrid?.IsEnabledForNonPublicFields ?? false;
            settings.PreserveExistingSummaryText = optionsGrid?.PreserveExistingSummaryText ?? true;
            settings.UseNaturalLanguageForReturnNode = optionsGrid?.UseNaturalLanguageForReturnNode ?? false;
            settings.UseToDoCommentsOnSummaryError = optionsGrid?.UseToDoCommentsOnSummaryError ?? false;
            settings.TryToIncludeCrefsForReturnTypes = optionsGrid?.TryToIncludeCrefsForReturnTypes ?? false;
            settings.WordMaps = optionsGrid?.WordMaps ?? Constants.DEFAULT_WORD_MAPS;
        }

        public static ISettings Update(this ISettings settings, ISettings newSettings, IEventLogger logger)
        {
            settings.IsEnabledForPublicMembersOnly = newSettings.IsEnabledForPublicMembersOnly;
            settings.UseNaturalLanguageForReturnNode = newSettings.UseNaturalLanguageForReturnNode;
            settings.ExcludeAsyncSuffix = newSettings.ExcludeAsyncSuffix;
            settings.IncludeValueNodeInProperties = newSettings.IncludeValueNodeInProperties;
            settings.UseToDoCommentsOnSummaryError = newSettings.UseToDoCommentsOnSummaryError;
            settings.WordMaps = newSettings.WordMaps;
            settings.DefaultDiagnosticSeverity = newSettings.DefaultDiagnosticSeverity;
            settings.PreserveExistingSummaryText = newSettings.PreserveExistingSummaryText;
            settings.ClassDiagnosticSeverity = newSettings.ClassDiagnosticSeverity;
            settings.ConstructorDiagnosticSeverity = newSettings.ConstructorDiagnosticSeverity;
            settings.EnumDiagnosticSeverity = newSettings.EnumDiagnosticSeverity;
            settings.FieldDiagnosticSeverity = newSettings.FieldDiagnosticSeverity;
            settings.InterfaceDiagnosticSeverity = newSettings.InterfaceDiagnosticSeverity;
            settings.MethodDiagnosticSeverity = newSettings.MethodDiagnosticSeverity;
            settings.PropertyDiagnosticSeverity = newSettings.PropertyDiagnosticSeverity;
            settings.RecordDiagnosticSeverity = newSettings.RecordDiagnosticSeverity;
            settings.IsEnabledForNonPublicFields = newSettings.IsEnabledForNonPublicFields;
            settings.TryToIncludeCrefsForReturnTypes = newSettings.TryToIncludeCrefsForReturnTypes;

            logger.LogInfo(JsonConvert.SerializeObject(settings), 200, 0, "Options updated");
            return settings;
        }

        /// <summary>
        ///  Gets the settings file path.
        /// </summary>
        /// <returns> A string. </returns>
        private static string GetSettingsFilePath()
        {
            const string name = "codedocumentor.json";
            var settingsPath = Path.Combine(_programDataFolder, name);

            if (!File.Exists(settingsPath))
            {
                new Settings().SaveToFile(settingsPath);
            }

            return settingsPath;
        }
    }
}
