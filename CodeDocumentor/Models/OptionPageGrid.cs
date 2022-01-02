using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Collections.Generic;
// For definitions of XML nodes see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
// see also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    //This has to live in this project so context thread is valid
    public class OptionPageGrid : DialogPage, IOptionPageGrid
    {
        [Category("CodeDocumentor")]
        [DisplayName("Enable comments for public members only")]
        [Description("When documenting classes, fields, methods, and properties only add documentation headers if the item is public")]
        public bool IsEnabledForPublishMembersOnly { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Use natural language for return comments")]
        [Description("When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example: <retrun>A List of Strings</return>")]
        public bool UseNaturalLanguageForReturnNode { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Exclude async wording from comments")]
        [Description("When documenting members skip adding asynchronously to the comment.")]
        public bool ExcludeAsyncSuffix { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Include <value> node in property comments")]
        [Description("When documenting properties add the value node with the return type")]
        public bool IncludeValueNodeInProperties { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Use TODO comment when summary can not be determined")]
        [Description("When documenting methods that can not create a valid summary insert TODO instead. Async is ignored")]
        public bool UseToDoCommentsOnSummaryError { get; set; }

        [Category("CodeDocumentor")]
        [DisplayName("Word mappings for creating comments")]
        [Description("When documenting if certain word are matched it will swap out to the translated mapping.")]
        public List<WordMap> WordMaps { get; set; } = Constants.WORD_MAPS;

    }

}
