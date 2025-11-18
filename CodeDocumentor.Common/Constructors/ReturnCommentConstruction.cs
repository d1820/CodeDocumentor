using System;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Common.Constructors
{
    /// <summary>
    ///  The return comment construction.
    /// </summary>
    public class ReturnCommentConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary>
        ///  Gets or Sets the dictionary comment template.
        /// </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "a dictionary with a key of type {0} and a value of type {1}";

        public ReturnCommentConstruction(TypeSyntax returnType, bool useNaturalLanguageForReturnNode, bool tryToIncludeCrefsForReturnTypes, WordMap[] wordMaps)
        {
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = !useNaturalLanguageForReturnNode,
                TryToIncludeCrefsForReturnTypes = tryToIncludeCrefsForReturnTypes,
                IncludeStartingWordInText = true,
                UseProperCasing = true
            };
            BuildReturnComment(returnType, options, wordMaps);
        }

        public ReturnCommentConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            BuildReturnComment(returnType, options, wordMaps);
        }

        //used for testing
        public ReturnCommentConstruction()
        {
        }

        private void BuildReturnComment(TypeSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            var comment = BuildComment(returnType, options, wordMaps).Trim();
            if (options.IncludeStartingWordInText && !options.ReturnGenericTypeAsFullString)
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment).Trim();
                }
            }
            else
            {
                Comment = comment;
            }
            if (options.UseProperCasing && !Comment.IsXml())
            {
                Comment = Comment.ToTitleCase();
            }
        }
    }
}
