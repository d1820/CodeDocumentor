using System;
using System.Collections.Generic;
using System.IO;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public class Settings : IOptionPageGrid
    {
        //This impl was adopted from https://github.com/mike-ward/VSColorOutput64/tree/db549b54709ca77ae5538c4046c332f1e51f90e7

        public bool ExcludeAsyncSuffix { get; set; }

        public bool IncludeValueNodeInProperties { get; set; }

        public bool IsEnabledForPublishMembersOnly { get; set; }

        public bool UseNaturalLanguageForReturnNode { get; set; }

        public bool UseToDoCommentsOnSummaryError { get; set; }

        public List<WordMap> WordMaps { get; set; } = Constants.WORD_MAPS;

        //public const string RegistryPath = @"DialogPage\BlueOnionSoftware.VsColorOutputOptions";

        private static readonly string ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeDocumentor");

        public static event EventHandler SettingsUpdated;

        private static void OnSettingsUpdated(object sender, EventArgs ea) => SettingsUpdated?.Invoke(sender, ea);

        private static string GetSettingsFilePath()
        {
            const string name = "codedocumentor.json";
            var settingsPath = Path.Combine(ProgramDataFolder, name);

            if (!File.Exists(settingsPath))
            {
                new Settings().SaveToFile(settingsPath);
            }

            return settingsPath;
        }

        public static Settings Load()
        {
            if (Runtime.RunningUnitTests) return new Settings();
            Directory.CreateDirectory(ProgramDataFolder);
            var json = File.ReadAllText(GetSettingsFilePath());
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
            return settings;
        }

        public void Save()
        {
            if (Runtime.RunningUnitTests) return;
            Directory.CreateDirectory(ProgramDataFolder);
            SaveToFile(GetSettingsFilePath());
            OnSettingsUpdated(this, EventArgs.Empty);
        }

        public void SaveToFile(string path)
        {
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
    }
}
