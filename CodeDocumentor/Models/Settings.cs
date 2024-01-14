using System;
using System.IO;
using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public class Settings : IOptionPageGrid
    {
        //This impl was adopted from https://github.com/mike-ward/VSColorOutput64/tree/db549b54709ca77ae5538c4046c332f1e51f90e7

        /// <summary> Gets or Sets the default diagnostic severity. </summary>
        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = DiagnosticSeverity.Warning;

        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary> Gets or Sets a value indicating whether exclude asynchronously suffix. </summary>
        /// <value> A bool. </value>
        public bool ExcludeAsyncSuffix { get; set; }

        /// <summary> Gets or Sets a value indicating whether include value node in properties. </summary>
        /// <value> A bool. </value>
        public bool IncludeValueNodeInProperties { get; set; }

        /// <summary> Gets or Sets a value indicating whether enabled for publish members is only. </summary>
        /// <value> A bool. </value>
        public bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        public bool IsEnabledForNonPublicFields { get; set; }

        /// <summary> Gets or Sets a value indicating whether preserve existing summary text. </summary>
        public bool PreserveExistingSummaryText { get; set; } = true;

        /// <summary> Gets or Sets a value indicating whether use natural language for return node. </summary>
        /// <value> A bool. </value>
        public bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary> Gets or Sets a value indicating whether use to do comments on summary error. </summary>
        /// <value> A bool. </value>
        public bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary> Gets or Sets the word maps. </summary>
        /// <value> An array of wordmaps. </value>
        public WordMap[] WordMaps { get; set; } = Constants.WORD_MAPS;

        //public const string RegistryPath = @"DialogPage\BlueOnionSoftware.VsColorOutputOptions";

        public static event EventHandler SettingsUpdated;

        /// <summary> Loads the <see cref="Settings" />. </summary>
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

        /// <summary> Saves the settings </summary>
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

        /// <summary> Saves the settings to file. </summary>
        /// <param name="path"> The path. </param>
        public void SaveToFile(string path)
        {
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }

        private static readonly string _programDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeDocumentor");

        /// <summary> Gets the settings file path. </summary>
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

        /// <summary> Ons the settings updated. </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="ea"> The ea. </param>
        private static void OnSettingsUpdated(object sender, EventArgs ea) => SettingsUpdated?.Invoke(sender, ea);
    }
}
