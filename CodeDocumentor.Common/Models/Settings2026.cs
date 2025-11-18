
using System.Collections.Generic;
using CodeDocumentor.Common.Interfaces;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Common.Models
{
    public class Settings2026 : IBaseSettings
    {
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

        public static IBaseSettings BuildDefaults()
        {
            return new Settings2026
            {
                ExcludeAsyncSuffix = false,
                IncludeValueNodeInProperties = false,
                IsEnabledForNonPublicFields = false,
                IsEnabledForPublicMembersOnly = false,
                PreserveExistingSummaryText = true,
                TryToIncludeCrefsForReturnTypes = true,
                UseNaturalLanguageForReturnNode = false,
                UseToDoCommentsOnSummaryError = true,
                WordMaps = Constants.DEFAULT_WORD_MAPS
            };
        }

        public IBaseSettings Clone()
        {
            var newSettings = new Settings2026
            {
                ExcludeAsyncSuffix = ExcludeAsyncSuffix,
                IncludeValueNodeInProperties = IncludeValueNodeInProperties,
                IsEnabledForNonPublicFields = IsEnabledForNonPublicFields,
                IsEnabledForPublicMembersOnly = IsEnabledForPublicMembersOnly,
                PreserveExistingSummaryText = PreserveExistingSummaryText,
                TryToIncludeCrefsForReturnTypes = TryToIncludeCrefsForReturnTypes,
                UseNaturalLanguageForReturnNode = UseNaturalLanguageForReturnNode,
                UseToDoCommentsOnSummaryError = UseToDoCommentsOnSummaryError
            };
            var clonedMaps = new List<WordMap>();
            foreach (var item in WordMaps)
            {
                clonedMaps.Add(new WordMap
                {
                    Translation = item.Translation,
                    Word = item.Word,
                    WordEvaluator = item.WordEvaluator
                });
            }
            newSettings.WordMaps = clonedMaps.ToArray();
            return newSettings;
        }
    }
}
