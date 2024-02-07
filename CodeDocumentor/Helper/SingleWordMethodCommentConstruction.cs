using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public class SingleWordMethodCommentConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary> Gets or Sets the array comment template. </summary>
        /// <value> A string. </value>
        public override string ArrayCommentTemplate { get; } = "an array of {0}";

        /// <summary> Gets or Sets the dictionary comment template. </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "dictionary with a key of type {0} and a value of type {1}";

        /// <summary> Gets or Sets the list comment template. </summary>
        /// <value> A string. </value>
        public override string ListCommentTemplate { get; } = "list of {0}";

        /// <summary> Gets or Sets the read only collection comment template. </summary>
        /// <value> A string. </value>
        public override string ReadOnlyCollectionCommentTemplate { get; } = "read only collection of {0}";

        /// <summary> Initializes a new instance of the <see cref="SingleWordMethodCommentConstruction" /> class. </summary>
        /// <param name="returnType"> The return type. </param>
        public SingleWordMethodCommentConstruction(TypeSyntax returnType) : base(false)
        {
            var comment = BuildComment(returnType, false); //we dont need to translate or period here cause the caller of this does all that work
            if (!string.IsNullOrEmpty(comment) && comment != ".")
            {
                Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), false), comment);
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
            if (returnType is PredefinedTypeSyntax)
            {
                return string.Empty;
            }
            var comment = base.BuildComment(returnType, returnGenericTypeAsFullString);
            //if (!returnGenericTypeAsFullString)
            //{
            //    comment = comment.Translate().WithPeriod();
            //}
            return comment;
            //return string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), false), comment);
        }

        /// <summary> Generates identifier name type comment. </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        internal override string GenerateIdentifierNameTypeComment(IdentifierNameSyntax returnType)
        {
            return $"<see cref=\"{returnType.Identifier.ValueText.Translate()}\"/>";
        }

        /// <summary> Generates qualified name type comment. </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        internal override string GenerateQualifiedNameTypeComment(QualifiedNameSyntax returnType)
        {
            return $"<see cref=\"{returnType.Translate()}\"/>";
        }
    }
}
