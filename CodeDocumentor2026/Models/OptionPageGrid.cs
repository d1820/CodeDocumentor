using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.VisualStudio.Shell;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor2026.Models
{
    //This has to live in this project so context thread is valid
    /// <summary>
    ///  The option page grid.
    /// </summary>
    [Guid("93DFF1A0-6A9F-42C6-9845-7F1E56105AE3")]
    public class OptionPageGrid : DialogPage, IBaseSettings
    {
        /// <summary>
        ///  The category.
        /// </summary>
        public const string Category = "CodeDocumentor2026";

        /// <summary>
        ///  The sub category.
        /// </summary>
        public const string SubCategory = "General";

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
        ///  Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        [Category(SummarySubCategory)]
        [DisplayName("Exclude async wording from comments")]
        [Description("When documenting members skip adding asynchronously to the comment.")]
        public bool ExcludeAsyncSuffix { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        [Category(ReturnsSubCategory)]
        [DisplayName("Include <value> node in property comments")]
        [Description("When documenting properties add the value node with the return type")]
        public bool IncludeValueNodeInProperties { get; set; }

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
        ///  Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        [Category(SummarySubCategory)]
        [DisplayName("Preserve Existing Summary Text")]
        [Description("When updating a comment or documenting the whole file if this is true; the summary text will not be regenerated. Defaults to true.")]
        public bool PreserveExistingSummaryText { get; set; } = true;

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

        /// <summary>
        ///  Load settings from storage.
        /// </summary>
        public override void LoadSettingsFromStorage()
        {
            IBaseSettings settings = new Settings2026();
            settings = settings.Load();
            IsEnabledForPublicMembersOnly = settings.IsEnabledForPublicMembersOnly;
            UseNaturalLanguageForReturnNode = settings.UseNaturalLanguageForReturnNode;
            ExcludeAsyncSuffix = settings.ExcludeAsyncSuffix;
            IncludeValueNodeInProperties = settings.IncludeValueNodeInProperties;
            UseToDoCommentsOnSummaryError = settings.UseToDoCommentsOnSummaryError;
            TryToIncludeCrefsForReturnTypes = settings.TryToIncludeCrefsForReturnTypes;
            WordMaps = settings.WordMaps ?? CodeDocumentor.Common.Constants.DEFAULT_WORD_MAPS;
            PreserveExistingSummaryText = settings.PreserveExistingSummaryText;
            IsEnabledForNonPublicFields = settings.IsEnabledForNonPublicFields;
        }

        /// <summary>
        ///  Save settings to storage.
        /// </summary>
        public override void SaveSettingsToStorage()
        {
            var settings = new Settings2026();
            var eventLogger = new Logger();
            settings.Update(this, eventLogger);
            settings.Save();
        }

        public ISettings Clone()
        {
            throw new NotImplementedException();
        }
    }
}
