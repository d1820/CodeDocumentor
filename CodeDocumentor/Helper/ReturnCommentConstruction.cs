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
        public override string ArrayCommentTemplate { get; } = "an array of {0}";

        /// <summary> Gets or Sets the dictionary comment template. </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; } = "a dictionary with a key of type {0} and a value of type {1}";

        /// <summary> Gets or Sets the list comment template. </summary>
        /// <value> A string. </value>
        public override string ListCommentTemplate { get; } = "a list of {0}";

        /// <summary> Gets or Sets the read only collection comment template. </summary>
        /// <value> A string. </value>
        public override string ReadOnlyCollectionCommentTemplate { get; } = "a read only collection of {0}";

        //used for testing
        internal ReturnCommentConstruction()
        {
        }

        public ReturnCommentConstruction(TypeSyntax returnType)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = !optionsService.UseNaturalLanguageForReturnNode
            };
            var comment = BuildComment(returnType, options);
            if (optionsService.UseNaturalLanguageForReturnNode)
            {
                comment = NameSplitter
                              .Split(comment)
                              .TranslateParts()
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations()
                              .WithPeriod();
                //comment = comment.ApplyUserTranslations().WithPeriod();
                if (!string.IsNullOrEmpty(comment))
                {
                    Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment).Trim();
                }
            }
            else
            {
                Comment = comment;
            }
        }

        public ReturnCommentConstruction(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            options.UseProperCasing = true;
            var comment = BuildComment(returnType, options);

            comment = NameSplitter
                              .Split(comment)
                              .TranslateParts()
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations()
                              .WithPeriod();

            if (!string.IsNullOrEmpty(comment))
            {
                Comment = string.Format("{0} {1}", DocumentationHeaderHelper.DetermineStartingWord(comment.AsSpan(), true), comment).Trim();
            }
        }

        internal override string BuildComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            return base.BuildComment(returnType, options).Trim();
        }
    }
}
