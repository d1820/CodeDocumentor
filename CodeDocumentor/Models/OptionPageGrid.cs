using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Extensions;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    //This has to live in this project so context thread is valid
    /// <summary>
    ///  The option page grid.
    /// </summary>
    [Guid("BE905985-26BB-492B-9453-743E26F4E8BB")]
    public class OptionPageGrid : DialogPage, ISettings
    {
        /// <summary>
        ///  The category.
        /// </summary>
        public const string Category = "CodeDocumentor";

        /// <summary>
        ///  The sub category.
        /// </summary>
        public const string SubCategory = "General";

        /// <summary>
        ///  The analyzer sub category.
        /// </summary>
        private const string AnalyzerSubCategory = "Analyzer Options";

        /// <summary>
        ///  The returns sub category.
        /// </summary>
        private const string ReturnsSubCategory = "Return Options";

        /// <summary>
        ///  The summary sub category.
        /// </summary>
        private const string SummarySubCategory = "Summary Options";

        /// <summary>
        ///  The translation sub category.
        /// </summary>
        private const string TranslationSubCategory = "Translation Options";

        /// <summary>
        ///  Gets or Sets the class diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Class Diagnostic Severity")]
        [Description("When highlighting missing comments on a class this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets the constructor diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Constructor Diagnostic Severity")]
        [Description("When highlighting missing comments on a constructor this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets the default diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Default Diagnostic Severity")]
        [Description("When highlighting missing comments this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = DiagnosticSeverity.Warning;

        /// <summary>
        ///  Gets or Sets the enum diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Enum Diagnostic Severity")]
        [Description("When highlighting missing comments on an enum this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        [Category(SummarySubCategory)]
        [DisplayName("Exclude async wording from comments")]
        [Description("When documenting members skip adding asynchronously to the comment.")]
        public bool ExcludeAsyncSuffix { get; set; }

        /// <summary>
        ///  Gets or Sets the field diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Field Diagnostic Severity")]
        [Description("When highlighting missing comments on a field this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        [Category(ReturnsSubCategory)]
        [DisplayName("Include <value> node in property comments")]
        [Description("When documenting properties add the value node with the return type")]
        public bool IncludeValueNodeInProperties { get; set; }

        /// <summary>
        ///  Gets or Sets the interface diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Interface Diagnostic Severity")]
        [Description("When highlighting missing comments on an interface this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        [Category(SubCategory)]
        [DisplayName("Enable comments for non public fields")]
        [Description("When documenting fields allow adding documentation headers if the item is not public. This only applies to const and static fields. Visual Studio must be restarted to fully take affect.")]
        public bool IsEnabledForNonPublicFields { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for publish members is only.
        /// </summary>
        /// <value> A bool. </value>
        [Category(SubCategory)]
        [DisplayName("Enable comments for public members only")]
        [Description("When documenting classes, fields, methods, and properties only add documentation headers if the item is public. Visual Studio must be restarted to fully take affect.")]
        public bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary>
        ///  Gets or Sets the method diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Method Diagnostic Severity")]
        [Description("When highlighting missing comments on a method this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        [Category(SummarySubCategory)]
        [DisplayName("Preserve Existing Summary Text")]
        [Description("When updating a comment or documenting the whole file if this is true; the summary text will not be regenerated. Defaults to true.")]
        public bool PreserveExistingSummaryText { get; set; } = true;

        /// <summary>
        ///  Gets or Sets the property diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Property Diagnostic Severity")]
        [Description("When highlighting missing comments on a property this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets the record diagnostic severity.
        /// </summary>
        [Category(AnalyzerSubCategory)]
        [DisplayName("Record Diagnostic Severity")]
        [Description("When highlighting missing comments on a record this is the default severity that gets applied. Visual Studio must be restarted to fully take affect.")]
        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use try and include crefs in method comments.
        /// </summary>
        /// <value> A bool. </value>
        [Category(SummarySubCategory)]
        [DisplayName("Try to include return types in documentation")]
        [Description("When documenting methods and properties (and Use natural language for return comments is enabled) try to include <cref/> in the return element. In methods that are named 2 words or less try and generate <cref/> elements for those types in the method comment")]
        public bool TryToIncludeCrefsForReturnTypes { get; set; }

        //Any properties that need defaults should be mnanaged in the Settings Class. This is only a pass through for VS
        /// <summary>
        ///  Gets or Sets a value indicating whether use natural language for return node.
        /// </summary>
        /// <value> A bool. </value>
        [Category(ReturnsSubCategory)]
        [DisplayName("Use natural language for return comments")]
        [Description("When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example: <return>A List of Strings</return>")]
        public bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use to do comments on summary error.
        /// </summary>
        /// <value> A bool. </value>
        [Category(SummarySubCategory)]
        [DisplayName("Use TODO comment when summary can not be determined")]
        [Description("When documenting methods that can not create a valid summary insert TODO instead. Async is ignored")]
        public bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary>
        ///  Gets or Sets the word maps.
        /// </summary>
        /// <value> An array of wordmaps. </value>
        [Category(TranslationSubCategory)]
        [DisplayName("Word mappings for creating comments")]
        [Description("When documenting if certain word are matched it will swap out to the translated mapping.")]
        public WordMap[] WordMaps { get; set; }

        [Category(AnalyzerSubCategory)]
        [DisplayName("Use .editorconfig for extension options")]
        [Description("This will convert existing extension options to .editorconfig values stored in %USERPROFILE%. This allows CodeDocumentor to run out of process.")]
        public bool UseEditorConfigForSettings { get; set; }


        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            var settings = new Settings();
            if (settings.IsCodeDocumentorDefinedInEditorConfig())
            {
                MessageBox.Show(
                  $"CodeDocumentor options have been detected in your %USERPROFILE% .editorconfig file. " +
                  $"Options are no longer managed through Visual Studio. ",
                  "CodeDocumentor Options Management",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Exclamation);
            }
        }
        /// <summary>
        ///  Load settings from storage.
        /// </summary>
        public override void LoadSettingsFromStorage()
        {
            var settings = new Settings();
            var f = this.Window.Handle;
            if (settings.IsCodeDocumentorDefinedInEditorConfig()) {
                UseEditorConfigForSettings = true;
                return;
            }
             settings = settings.Load();
            IsEnabledForPublicMembersOnly = settings.IsEnabledForPublicMembersOnly;
            UseNaturalLanguageForReturnNode = settings.UseNaturalLanguageForReturnNode;
            ExcludeAsyncSuffix = settings.ExcludeAsyncSuffix;
            IncludeValueNodeInProperties = settings.IncludeValueNodeInProperties;
            UseToDoCommentsOnSummaryError = settings.UseToDoCommentsOnSummaryError;
            TryToIncludeCrefsForReturnTypes = settings.TryToIncludeCrefsForReturnTypes;
            WordMaps = settings.WordMaps ?? Constants.DEFAULT_WORD_MAPS; //if we dont have anything need to use defaults
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
            UseEditorConfigForSettings = settings.UseEditorConfigForSettings;
        }

        /// <summary>
        ///  Save settings to storage.
        /// </summary>
        public override void SaveSettingsToStorage()
        {
            var settings = new Settings();
            if (settings.IsCodeDocumentorDefinedInEditorConfig())
            {
                MessageBox.Show(
                    $"CodeDocumentor options have been detected in your %USERPROFILE% .editorconfig file. " +
                    $"Options are no longer managed through Visual Studio. ",
                    "CodeDocumentor Options Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            settings = new Settings
            {
                IsEnabledForPublicMembersOnly = IsEnabledForPublicMembersOnly,
                UseNaturalLanguageForReturnNode = UseNaturalLanguageForReturnNode,
                ExcludeAsyncSuffix = ExcludeAsyncSuffix,
                IncludeValueNodeInProperties = IncludeValueNodeInProperties,
                UseToDoCommentsOnSummaryError = UseToDoCommentsOnSummaryError,
                TryToIncludeCrefsForReturnTypes = TryToIncludeCrefsForReturnTypes,
                WordMaps = WordMaps,
                DefaultDiagnosticSeverity = DefaultDiagnosticSeverity,
                PreserveExistingSummaryText = PreserveExistingSummaryText,
                ClassDiagnosticSeverity = ClassDiagnosticSeverity,
                ConstructorDiagnosticSeverity = ConstructorDiagnosticSeverity,
                EnumDiagnosticSeverity = EnumDiagnosticSeverity,
                FieldDiagnosticSeverity = FieldDiagnosticSeverity,
                InterfaceDiagnosticSeverity = InterfaceDiagnosticSeverity,
                MethodDiagnosticSeverity = MethodDiagnosticSeverity,
                PropertyDiagnosticSeverity = PropertyDiagnosticSeverity,
                RecordDiagnosticSeverity = RecordDiagnosticSeverity,
                IsEnabledForNonPublicFields = IsEnabledForNonPublicFields,
                UseEditorConfigForSettings = UseEditorConfigForSettings
            };
            var response = DialogResult.No;
            if (settings.UseEditorConfigForSettings)
            {
                response = MessageBox.Show(
                    $"This will convert existing extension options to .editorconfig values. " +
                    $"This allows CodeDocumentor to run out of process. " +
                    $"Do you want to continue?{Environment.NewLine}" +
                    $"You will need to paste these options into your %USERPROFILE% .editorconfig and restart Visual Studio.",
                    "CodeDocumentor Options Management",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
            }
            if (response == DialogResult.Yes)
            {
                settings.SaveToEditorConfig(clipboardStr => Clipboard.SetText(clipboardStr));
                MessageBox.Show("Settings have been copied to the clipboard. Update your .editorconfig file in %USERPROFILE% with the contents to use the new settings. You will need to restart Visual Studio.",
                    "Settings copied to clipboard",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            }
            settings.Save();
            var optionsService = CodeDocumentorPackage._optService;
            optionsService.Update(settings);
        }
    }
}
