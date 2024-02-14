using System;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Constructors
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

        public ReturnCommentConstruction(TypeSyntax returnType)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = !optionsService.UseNaturalLanguageForReturnNode,
                TryToIncludeCrefsForReturnTypes = optionsService.TryToIncludeCrefsForReturnTypes,
                IncludeStartingWordInText = true,
                UseProperCasing = true
            };
            BuildReturnComment(returnType, options);
        }

        public ReturnCommentConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            BuildReturnComment(returnType, options);
        }

        //used for testing
        internal ReturnCommentConstruction()
        {
        }

        private void BuildReturnComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var comment = BuildComment(returnType, options).Trim();
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
