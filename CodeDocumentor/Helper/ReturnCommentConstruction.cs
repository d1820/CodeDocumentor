using System;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary> The return comment construction. </summary>
    public class ReturnCommentConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary> Gets or Sets the array comment template. </summary>
        /// <value> A string. </value>
        public override string ArrayCommentTemplate { get; } = "An array of {0}";

        /// <summary> Gets or Sets the dictionary comment template. </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "A dictionary with a key of type {0} and a value of type {1}";

        /// <summary> Gets or Sets the list comment template. </summary>
        /// <value> A string. </value>
        public override string ListCommentTemplate { get; } = "A list of {0}";

        /// <summary> Gets or Sets the read only collection comment template. </summary>
        /// <value> A string. </value>
        public override string ReadOnlyCollectionCommentTemplate { get; } = "A read only collection of {0}";

        //used for testing
        internal ReturnCommentConstruction() : base(true)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="ReturnCommentConstruction" /> class. </summary>
        /// <param name="returnType"> The return type. </param>
        public ReturnCommentConstruction(TypeSyntax returnType) : base(true)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            var comment = BuildComment(returnType, !optionsService.UseNaturalLanguageForReturnNode);
            if (optionsService.UseNaturalLanguageForReturnNode)
            {
                comment = comment.ApplyUserTranslations().WithPeriod();
                if (!string.IsNullOrEmpty(comment) && comment != ".")
                {
                    Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment);
                }
            }
            else
            {
                Comment = comment;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnCommentConstruction"/> class.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <param name="returnGenericTypeAsFullString">If true, return generic type as full string.</param>
        public ReturnCommentConstruction(TypeSyntax returnType, bool returnGenericTypeAsFullString) : base(true)
        {
            var comment = BuildComment(returnType, returnGenericTypeAsFullString).ApplyUserTranslations().WithPeriod();
            if (!string.IsNullOrEmpty(comment) && comment != ".")
            {
                Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment);
            }
        }

        /// <summary> Builds a string comment for a summary node </summary>
        /// <param name="returnType"> </param>
        /// <param name="returnGenericTypeAsFullString">
        ///     Flag indicating if the full type should just be returned as a string
        /// </param>
        /// <returns> The comment </returns>
        internal override string BuildComment(TypeSyntax returnType, bool returnGenericTypeAsFullString)
        {
            return base.BuildComment(returnType, returnGenericTypeAsFullString).Trim();
        }
    }
}
