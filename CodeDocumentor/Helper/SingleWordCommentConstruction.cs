using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public class SingleWordCommentConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary> Gets or Sets the dictionary comment template. </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "dictionary with a key of type {0} and a value of type {1}";

        /// <summary> Initializes a new instance of the <see cref="SingleWordCommentConstruction" /> class. </summary>
        /// <param name="returnType"> The return type. </param>
        public SingleWordCommentConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var comment = BuildComment(returnType, options); //we dont need to translate or period here cause the caller of this does all that work
            if (!string.IsNullOrEmpty(comment))
            {
                Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), false), comment).Trim();
            }
        }

        internal override string BuildComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            if (options.ForcePredefinedTypeEvaluation)
            {
                return base.BuildComment(returnType, options);
            }
            if (returnType is PredefinedTypeSyntax)
            {
                return string.Empty;
            }
            var comment = base.BuildComment(returnType, options);
            return comment;
        }
    }
}
