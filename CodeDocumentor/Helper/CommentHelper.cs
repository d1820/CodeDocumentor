using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("CodeDocumentor.Test")]

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///  The comment helper.
    /// </summary>
    public class CommentHelper
    {
        /// <summary>
        ///  Creates the class comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateClassComment(string name, WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts(wordMaps)
                         .TryInsertTheWord((parts) =>
                         {
                             if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 parts.Insert(0, "The"); //for records we always force "The"
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(wordMaps)
                         .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the constructor comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isPrivate"> If true, is private. </param>
        /// <returns> A string. </returns>
        public string CreateConstructorComment(string name, bool isPrivate, WordMap[] wordMaps)
        {
            string comment;
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = name.Trim();
            if (isPrivate)
            {
                comment = $"Prevents a default instance of the <see cref=\"{name}\"/> class from being created";
            }
            else
            {
                comment = $"Initializes a new instance of the <see cref=\"{name}\"/> class";
            }
            return comment.ApplyUserTranslations(wordMaps).WithPeriod();
        }

        /// <summary>
        ///  Creates the enum comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateEnumComment(string name,  WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                           .Split(name)
                           .TranslateParts(wordMaps)
                           .TryPluarizeFirstWord()
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .PluaralizeLastWord()
                           .JoinToString()
                           .ApplyUserTranslations(wordMaps)
                           .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the field comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateFieldComment(string name, bool excludeAsyncSuffix, WordMap[] wordMaps)
        {
            //string comment;
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            //order matters. fields are special in the sense there is not action attached and we dont need to do translations
            var comment = NameSplitter
                            .Split(name)
                            .HandleAsyncKeyword(excludeAsyncSuffix)
                            .Tap((parts) =>
                            {
                                if (parts.Count > 0 && char.IsLower(parts[0], 0)) //if first letter of a field is lower its prob a private field. Lets adjust for it
                                {
                                    parts[0] = parts[0].ToTitleCase();
                                }
                            })
                            .TryInsertTheWord()
                            .ToLowerParts()
                            .JoinToString()
                            .ApplyUserTranslations(wordMaps)
                            .WithPeriod();

            return comment;
        }

        /// <summary>
        ///  Creates the interface comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateInterfaceComment(string name, WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                           .Split(name)
                           .Tap((parts) =>
                           {
                               parts.Remove("I");
                           })
                           .AddCustomPart("interface")
                           .TranslateParts(wordMaps)
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .JoinToString()
                           .ApplyUserTranslations(wordMaps)
                           .WithPeriod();

            return comment;
        }

        /// <summary>
        ///  Creates the method comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="returnType"> The return type. </param>
        /// <returns> A string. </returns>
        public string CreateMethodComment(string name, TypeSyntax returnType,
            bool useToDoCommentsOnSummaryError,
            bool tryToIncludeCrefsForReturnTypes,
            bool excludeAsyncSuffix,
            WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var isBool = false;
            var is2partPlusAsync = false;
            var hasReturnComment = true;
            var comment = NameSplitter
                         .Split(name)
                         .Tap((parts) =>
                         {
                             is2partPlusAsync = parts.Count == 2 || (parts.Count == 3 && parts.Last().StartsWith("async", StringComparison.InvariantCultureIgnoreCase));
                             isBool = returnType.IsBoolReturnType();
                         })
                         .HandleAsyncKeyword(excludeAsyncSuffix)
                         .TryIncludeReturnType(useToDoCommentsOnSummaryError, tryToIncludeCrefsForReturnTypes, wordMaps, returnType, (returnComment) =>
                         {
                             hasReturnComment = !string.IsNullOrEmpty(returnComment);
                         })
                         .TryAddTodoSummary(returnType.ToString(), useToDoCommentsOnSummaryError)
                         .TranslateParts(wordMaps)
                         .TryPluarizeFirstWord()
                         .TryInsertTheWord((parts) =>
                         {
                             //this means any name of a method that is not a return type of bool and not 2 part (+async) should insert "the" into the sentence
                             if (!isBool &&
                                !hasReturnComment &&
                                !Constants.EXCLUDE_THE_LIST_FOR_2PART_COMMENTS.Any(a => a.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase)) &&
                                is2partPlusAsync)
                             {
                                 parts.Insert(1, "the");
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(wordMaps)
                         .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the parameter comment.
        /// </summary>
        /// <param name="parameter"> The parameter. </param>
        /// <returns> A string. </returns>
        public string CreateParameterComment(ParameterSyntax parameter, WordMap[] wordMaps)
        {
            var isBoolean = false;
            if (parameter.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = (parameter.Type as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (parameter.Type.IsKind(SyntaxKind.NullableType))
            {
                // If it is not predefined type syntax, it should be IdentifierNameSyntax.
                if ((parameter.Type as NullableTypeSyntax)?.ElementType is PredefinedTypeSyntax type)
                {
                    isBoolean = type.Keyword.IsKind(SyntaxKind.BoolKeyword);
                }
            }

            if (isBoolean)
            {
                var comment = NameSplitter
                                .Split(parameter.Identifier.ValueText)
                                .AddCustomPart("If true,", 0)
                                .TranslateParts(wordMaps)
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations(wordMaps)
                                .WithPeriod();
                return comment;
            }
            else
            {
                var comment = NameSplitter
                                .Split(parameter.Identifier.ValueText)
                                .TranslateParts(wordMaps)
                                .TryInsertTheWord((parts) =>
                                {
                                    if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        parts.Insert(0, "The"); //for parameters we always force "The"
                                    }
                                })
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations(wordMaps)
                                .WithPeriod();
                return comment;
            }
        }

        /// <summary>
        ///  Creates the property comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isBoolean"> If true, is boolean. </param>
        /// <param name="hasSetter"> If true, has setter. </param>
        /// <returns> A string. </returns>
        public string CreatePropertyComment(string name, bool isBoolean, bool hasSetter, bool excludeAsyncSuffix, WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            if (isBoolean)
            {
                var comment = NameSplitter
                              .Split(name)
                              .AddPropertyBooleanPart() //we do this here cause it will get pushed down the stack
                              .AddCustomPart("Gets", 0)
                              .AddCustomPart(hasSetter ? "or Sets" : null, 1)
                              .TranslateParts(wordMaps)
                              .HandleAsyncKeyword(excludeAsyncSuffix)
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations(wordMaps)
                              .WithPeriod();
                return comment;
            }
            else
            {
                var comment = NameSplitter
                              .Split(name)
                              .AddCustomPart("the", 0) //we do this here cause it will get pushed down the stack
                              .AddCustomPart("Gets", 0)
                              .AddCustomPart(hasSetter ? "or Sets" : null, 1)
                              .TranslateParts(wordMaps)
                              .HandleAsyncKeyword(excludeAsyncSuffix)
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations(wordMaps)
                              .WithPeriod();
                return comment;
            }
        }

        /// <summary>
        ///  Create the record comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateRecordComment(string name, WordMap[] wordMaps)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts(wordMaps)
                         .TryInsertTheWord((parts) =>
                         {
                             if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 parts.Insert(0, "The"); //for records we always force "The"
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(wordMaps)
                         .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Has comment.
        /// </summary>
        /// <param name="commentTriviaSyntax"> The comment trivia syntax. </param>
        /// <returns> A bool. </returns>
        public bool HasComment(DocumentationCommentTriviaSyntax commentTriviaSyntax)
        {
            if (commentTriviaSyntax == null)
            {
                return false;
            }
            var hasSummary = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlElementSyntax>()
                .Any(o => o.StartTag.Name.ToString().Equals(Constants.SUMMARY));

            var hasInheritDoc = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlEmptyElementSyntax>()
                .Any(o => o.Name.ToString().Equals(Constants.INHERITDOC));

            return hasSummary || hasInheritDoc;
        }
    }
}
