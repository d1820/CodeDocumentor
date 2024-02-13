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
                //BuildWithPeriodAndPrefixForTaskTypes = false,
                TryToIncludeCrefsForReturnTypes = optionsService.TryToIncludeCrefsForReturnTypes,
                //IncludeReturnStatementInGeneralComments = returnType.GetType() != typeof(GenericNameSyntax)
                IncludeStartingWordInText = true,
                ReturnBuildType = ReturnBuildType.ReturnXmlElement,
                UseProperCasing = true
            };
            BuildReturnComment(returnType, options);
        }

        public ReturnCommentConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            BuildReturnComment(returnType, options);
        }

        private void BuildReturnComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var comment = BuildComment(returnType, options).Trim();
            if (!options.ReturnGenericTypeAsFullString)
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment).Trim();
                    if (options.UseProperCasing)
                    {
                        Comment = Comment.ToTitleCase();
                    }
                }
            }
            else
            {
                Comment = comment;
            }
        }

        //used for testing
        internal ReturnCommentConstruction()
        {
        }
    }
}
