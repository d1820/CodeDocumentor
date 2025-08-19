using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public class Settings : IOptionPageGrid
    {
        const string PREFIX = "codedocumentor_";

        //This impl was adopted from https://github.com/mike-ward/VSColorOutput64/tree/db549b54709ca77ae5538c4046c332f1e51f90e7

        private static readonly string _programDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeDocumentor");

        private static readonly string _userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = DiagnosticSeverity.Warning;

        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        public bool ExcludeAsyncSuffix { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        public bool IncludeValueNodeInProperties { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for publish members is only.
        /// </summary>
        /// <value> A bool. </value>
        public bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        public bool IsEnabledForNonPublicFields { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        public bool PreserveExistingSummaryText { get; set; } = true;

        /// <summary>
        ///  Gets or Sets a value indicating whether use try and include crefs in method comments.
        /// </summary>
        /// <value> A bool. </value>
        public bool TryToIncludeCrefsForReturnTypes { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use natural language for return node.
        /// </summary>
        /// <value> A bool. </value>
        public bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use to do comments on summary error.
        /// </summary>
        /// <value> A bool. </value>
        public bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary>
        ///  Gets or Sets the word maps.
        /// </summary>
        /// <value> An array of wordmaps. </value>
        public WordMap[] WordMaps { get; set; } = Constants.DEFAULT_WORD_MAPS;

        /// <summary>
        /// Gets or Sets a value indicating whether to use the .editorconfig file for settings.
        /// </summary>
        /// <remarks>
        /// This will convert the existing settings to a %USERPROFILE% .editorconfig file
        /// </remarks>
        public bool UseEditorConfigForSettings { get; set; }

        public static event EventHandler SettingsUpdated;

        /// <summary>
        ///  Loads the <see cref="Settings"/>.
        /// </summary>
        /// <returns> A Settings. </returns>
        public static Settings Load()
        {
            if (Runtime.RunningUnitTests)
            {
                return new Settings();
            }

            Directory.CreateDirectory(_programDataFolder);
            var json = File.ReadAllText(GetSettingsFilePath());
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
            return settings;
        }

        public static bool IsCodeDocumentorDefinedInEditorConfig()
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
        ///  Saves the settings
        /// </summary>
        public void Save()
        {
            if (Runtime.RunningUnitTests)
            {
                return;
            }


            Directory.CreateDirectory(_programDataFolder);
            SaveToFile(GetSettingsFilePath());
            OnSettingsUpdated(this, EventArgs.Empty);
        }

        public void SaveToEditorConfig()
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

            clipboardLInes.Add($"{PREFIX}class_diagram_severity = {ClassDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}constructor_diagram_severity = {ConstructorDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}default_diagram_severity = {DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}enum_diagram_severity = {EnumDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}field_diagram_severity = {FieldDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}interface_diagram_severity = {InterfaceDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}method_diagram_severity = {MethodDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}property_diagram_severity = {PropertyDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}record_diagram_severity = {RecordDiagnosticSeverity ?? DefaultDiagnosticSeverity}");
            clipboardLInes.Add($"{PREFIX}exclude_async_suffix = {ExcludeAsyncSuffix}");
            clipboardLInes.Add($"{PREFIX}include_value_node_in_properties = {IncludeValueNodeInProperties}");
            clipboardLInes.Add($"{PREFIX}is_enabled_for_public_members_only = {IsEnabledForPublicMembersOnly}");
            clipboardLInes.Add($"{PREFIX}is_enabled_for_non_public_fields = {IsEnabledForNonPublicFields}");
            clipboardLInes.Add($"{PREFIX}preserve_existing_summary_text = {PreserveExistingSummaryText}");
            clipboardLInes.Add($"{PREFIX}try_to_include_crefs_for_return_types = {TryToIncludeCrefsForReturnTypes}");
            clipboardLInes.Add($"{PREFIX}use_natural_language_for_return_node = {UseNaturalLanguageForReturnNode}");
            clipboardLInes.Add($"{PREFIX}use_todo_comments_on_summary_error = {UseToDoCommentsOnSummaryError}");

            if (WordMaps != null && WordMaps.Length > 0)
            {
                for (var i = 0; i < WordMaps.Length; i++)
                {
                    var word = WordMaps[i]?.Word ?? string.Empty;
                    var translation = WordMaps[i]?.Translation ?? string.Empty;
                    clipboardLInes.Add($"{PREFIX}wordmap_{i} = {word}:{translation}");
                }
            }
            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }

        /// <summary>
        ///  Saves the settings to file.
        /// </summary>
        /// <param name="path"> The path. </param>
        public void SaveToFile(string path)
        {
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
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

        /// <summary>
        ///  Ons the settings updated.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="ea"> The ea. </param>
        private static void OnSettingsUpdated(object sender, EventArgs ea) => SettingsUpdated?.Invoke(sender, ea);
    }
}
