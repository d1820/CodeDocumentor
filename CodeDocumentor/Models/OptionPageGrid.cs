using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
// For definitions of XML nodes see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
// see also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    //This has to live in this project so context thread is valid
    [Guid("BE905985-26BB-492B-9453-743E26F4E8BB")]
    public class OptionPageGrid : DialogPage, IOptionPageGrid
    {
        public const string Category = "CodeDocumentor";
        public const string SubCategory = "General";

        private const string SummarySubCategory = "Summary Options";
        private const string ReturnsSubCategory = "Return Options";
        private const string TranslationSubCategory = "Translation Options";

        //Any properties that need defaults should be mnanaged in the Settings Class. This is only a pass through for VS

        [Category(SubCategory)]
        [DisplayName("Enable comments for public members only")]
        [Description("When documenting classes, fields, methods, and properties only add documentation headers if the item is public")]
        public bool IsEnabledForPublishMembersOnly { get; set; }

        [Category(ReturnsSubCategory)]
        [DisplayName("Use natural language for return comments")]
        [Description("When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example: <retrun>A List of Strings</return>")]
        public bool UseNaturalLanguageForReturnNode { get; set; }

        [Category(SummarySubCategory)]
        [DisplayName("Exclude async wording from comments")]
        [Description("When documenting members skip adding asynchronously to the comment.")]
        public bool ExcludeAsyncSuffix { get; set; }

        [Category(ReturnsSubCategory)]
        [DisplayName("Include <value> node in property comments")]
        [Description("When documenting properties add the value node with the return type")]
        public bool IncludeValueNodeInProperties { get; set; }

        [Category(SummarySubCategory)]
        [DisplayName("Use TODO comment when summary can not be determined")]
        [Description("When documenting methods that can not create a valid summary insert TODO instead. Async is ignored")]
        public bool UseToDoCommentsOnSummaryError { get; set; }

        [Category(TranslationSubCategory)]
        [DisplayName("Word mappings for creating comments")]
        [Description("When documenting if certain word are matched it will swap out to the translated mapping.")]
        public List<WordMap> WordMaps { get; set; }



        public override void LoadSettingsFromStorage()
        {
            var settings = Settings.Load();
            IsEnabledForPublishMembersOnly = settings.IsEnabledForPublishMembersOnly;
            UseNaturalLanguageForReturnNode = settings.UseNaturalLanguageForReturnNode;
            ExcludeAsyncSuffix = settings.ExcludeAsyncSuffix;
            IncludeValueNodeInProperties = settings.IncludeValueNodeInProperties;
            UseToDoCommentsOnSummaryError = settings.UseToDoCommentsOnSummaryError;
            WordMaps = settings.WordMaps;
        }

        public override void SaveSettingsToStorage()
        {
            var settings = new Settings
            {
                IsEnabledForPublishMembersOnly = this.IsEnabledForPublishMembersOnly,
                UseNaturalLanguageForReturnNode = this.UseNaturalLanguageForReturnNode,
                ExcludeAsyncSuffix = this.ExcludeAsyncSuffix,
                IncludeValueNodeInProperties = this.IncludeValueNodeInProperties,
                UseToDoCommentsOnSummaryError = this.UseToDoCommentsOnSummaryError,
                WordMaps = this.WordMaps
            };
            settings.Save();
        }
    }
}
