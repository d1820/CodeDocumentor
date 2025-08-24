using System;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Constructors
{
    public class SingleWordCommentSummaryConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary>
        ///  Gets or Sets the dictionary comment template.
        /// </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "dictionary with a key of type {0} and a value of type {1}";

        /// <summary>
        ///  Initializes a new instance of the <see cref="SingleWordCommentSummaryConstruction"/> class.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        public SingleWordCommentSummaryConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            var comment = BuildComment(returnType, options, wordMaps); //we dont need to translate or period here cause the caller of this does all that work
            if (!string.IsNullOrEmpty(comment))
            {
                var startWord = "";
                if (options.IncludeStartingWordInText)
                {
                    startWord = DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), false);
                }
                var builtComment = string.Format("{0} {1}", startWord, comment).Trim();
                if (!string.IsNullOrEmpty(builtComment))
                {
                    if (builtComment.StartsWith("an "))
                    {
                        Comment = "and returns " + builtComment;
                        return;
                    }
                    Comment = "and return " + builtComment;
                    return;
                }
                Comment = null;
            }
        }
    }
}
