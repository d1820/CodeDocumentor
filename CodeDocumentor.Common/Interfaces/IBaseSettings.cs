using CodeDocumentor.Common.Models;

namespace CodeDocumentor.Common.Interfaces
{
    public interface IBaseSettings
    {
        /// <summary>
        ///   Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        bool ExcludeAsyncSuffix { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        bool IncludeValueNodeInProperties { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        bool IsEnabledForNonPublicFields { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether enabled for public members is only.
        /// </summary>
        /// <value> A bool. </value>
        bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        bool PreserveExistingSummaryText { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether use try and include return type crefs in documentation.
        /// </summary>
        /// <value> A bool. </value>
        bool TryToIncludeCrefsForReturnTypes { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether use natural language for return node.
        /// </summary>
        /// <value> A bool. </value>
        bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether use to do comments on summary error.
        /// </summary>
        /// <value> A bool. </value>
        bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary>
        ///   Gets or Sets the word maps.
        /// </summary>
        /// <value> A list of wordmaps. </value>
        WordMap[] WordMaps { get; set; }
    }
}
