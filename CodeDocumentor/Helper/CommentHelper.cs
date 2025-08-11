using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using CodeDocumentor.Constructors;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
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
        public string CreateClassComment(string name, IOptionsService optionsService)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts(optionsService)
                         .TryInsertTheWord((parts) =>
                         {
                             if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 parts.Insert(0, "The"); //for records we always force "The"
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(optionsService.WordMaps)
                         .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the constructor comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isPrivate"> If true, is private. </param>
        /// <returns> A string. </returns>
        public string CreateConstructorComment(string name, bool isPrivate, IOptionsService optionsService)
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
            return comment.ApplyUserTranslations(optionsService.WordMaps).WithPeriod();
        }

        /// <summary>
        ///  Creates the enum comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateEnumComment(string name, IOptionsService optionsService)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                           .Split(name)
                           .TranslateParts(optionsService)
                           .TryPluarizeFirstWord()
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .PluaralizeLastWord()
                           .JoinToString()
                           .ApplyUserTranslations(optionsService.WordMaps)
                           .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the field comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateFieldComment(string name, IOptionsService optionsService)
        {
            //string comment;
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            //order matters. fields are special in the sense there is not action attached and we dont need to do translations
            var comment = NameSplitter
                            .Split(name)
                            .HandleAsyncKeyword(optionsService)
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
                            .ApplyUserTranslations(optionsService.WordMaps)
                            .WithPeriod();

            return comment;
        }

        /// <summary>
        ///  Creates the interface comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateInterfaceComment(string name, IOptionsService optionsService)
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
                           .TranslateParts(optionsService)
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .JoinToString()
                           .ApplyUserTranslations(optionsService.WordMaps)
                           .WithPeriod();

            return comment;
        }

        /// <summary>
        ///  Creates the method comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="returnType"> The return type. </param>
        /// <returns> A string. </returns>
        public string CreateMethodComment(string name, TypeSyntax returnType, IOptionsService optionsService)
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
                         .HandleAsyncKeyword(optionsService)
                         .TryIncudeReturnType(optionsService, returnType, (returnComment) =>
                         {
                             hasReturnComment = !string.IsNullOrEmpty(returnComment);
                         })
                         .TryAddTodoSummary(returnType.ToString(), optionsService)
                         .TranslateParts(optionsService)
                         .TryPluarizeFirstWord()
                         .TryInsertTheWord((parts) =>
                         {
                             //this means any name of a method that is not a return type of bool and not 2 part (+async) should insert "the" into the sentence
                             if (!isBool &&
                                !hasReturnComment &&
                                !Vsix2022.Constants.EXCLUDE_THE_LIST_FOR_2PART_COMMENTS.Any(a => a.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase)) &&
                                is2partPlusAsync)
                             {
                                 parts.Insert(1, "the");
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(optionsService.WordMaps)
                         .WithPeriod();
            return comment;
        }

        /// <summary>
        ///  Creates the parameter comment.
        /// </summary>
        /// <param name="parameter"> The parameter. </param>
        /// <returns> A string. </returns>
        public string CreateParameterComment(ParameterSyntax parameter, IOptionsService optionsService)
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
                                .TranslateParts(optionsService)
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations(optionsService.WordMaps)
                                .WithPeriod();
                return comment;
            }
            else
            {
                var comment = NameSplitter
                                .Split(parameter.Identifier.ValueText)
                                .TranslateParts(optionsService)
                                .TryInsertTheWord((parts) =>
                                {
                                    if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        parts.Insert(0, "The"); //for parameters we always force "The"
                                    }
                                })
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations(optionsService.WordMaps)
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
        public string CreatePropertyComment(string name, bool isBoolean, bool hasSetter, IOptionsService optionsService)
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
                              .TranslateParts(optionsService)
                              .HandleAsyncKeyword(optionsService)
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations(optionsService.WordMaps)
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
                              .TranslateParts(optionsService)
                              .HandleAsyncKeyword(optionsService)
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations(optionsService.WordMaps)
                              .WithPeriod();
                return comment;
            }
        }

        /// <summary>
        ///  Create the record comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public string CreateRecordComment(string name, IOptionsService optionsService)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts(optionsService)
                         .TryInsertTheWord((parts) =>
                         {
                             if (parts.Count > 0 && !parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                             {
                                 parts.Insert(0, "The"); //for records we always force "The"
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations(optionsService.WordMaps)
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
                .Any(o => o.StartTag.Name.ToString().Equals(Vsix2022.Constants.SUMMARY));

            var hasInheritDoc = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlEmptyElementSyntax>()
                .Any(o => o.Name.ToString().Equals(Vsix2022.Constants.INHERITDOC));

            return hasSummary || hasInheritDoc;
        }
    }

    public static class StringExtensions
    {

        public static string RemovePeriod(this string text)
        {
            return text?.Trim().EndsWith(".") == true ? text.Remove(text.Length - 1) : text;
        }

        /// <summary>
        ///  Withs the period.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> A string. </returns>
        public static string WithPeriod(this string text)
        {
            if (text?.Trim().EndsWith(".") == true)
            {
                return text;
            }
            return text.Length > 0 ? text + "." : text;
        }
    }

    public static class ListExtensions
    {
        public static List<string> AddCustomPart(this List<string> parts, string part = null, int idx = -1)
        {
            if (part is null)
            {
                return parts;
            }
            if (idx == -1)
            {
                parts.Add(part);
                return parts;
            }
            parts.Insert(idx, part);
            return parts;
        }

        public static string JoinToString(this List<string> parts, string delimiter = " ")
        {
            return $"{string.Join(delimiter, parts)}";
        }

        public static List<string> PluaralizeLastWord(this List<string> parts)
        {
            var lastIdx = parts.Count - 1;
            parts[lastIdx] = Pluralizer.ForcePluralization(parts[lastIdx]);
            return parts;
        }

        public static List<string> Tap(this List<string> parts, Action<List<string>> tapAction)
        {
            tapAction?.Invoke(parts);
            return parts;
        }

        public static List<string> ToLowerParts(this List<string> parts, bool forceFirstCharToLower = false)
        {
            var i = forceFirstCharToLower ||
                    (
                        parts.Count > 0 &&
                        !parts[0].Equals("The", StringComparison.InvariantCultureIgnoreCase) &&
                        !parts[0].Equals("If true,", StringComparison.InvariantCultureIgnoreCase) &&
                        !parts[0].IsVerb() //if the first word is a verb we are not adding The anyway so we need to leave it Pascal
                    )
                ? 0 : 1;

            parts.SwapXmlTokens((part) =>
            {
                if (!part.All(a => char.IsUpper(a)))
                {
                    part = part.ToLower();
                }
                return part;
            }, i);

            //First letter is always caps unless it was forced lower
            if (!forceFirstCharToLower && parts.Count > 0 && char.IsLower(parts[0], 0))
            {
                parts[0] = parts[0].ToTitleCase();
            }
            return parts;
        }

        public static List<string> TranslateParts(this List<string> parts, IOptionsService optionsService)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                var nextWord = i + 1 < parts.Count ? parts[i + 1] : null;
                var userMaps = optionsService.WordMaps ?? Array.Empty<WordMap>();
                foreach (var wordMap in Vsix2022.Constants.INTERNAL_WORD_MAPS)
                {
                    if (!CanEvaluateWordMap(wordMap, i))
                    {
                        continue;
                    }
                    //dont run an internal word map if the user has one for the same thing
                    if (!userMaps.Any(a => a.Word == wordMap.Word))
                    {
                        var wordToLookFor = string.Format(Vsix2022.Constants.WORD_MATCH_REGEX_TEMPLATE, wordMap.Word);
                        parts[i] = Regex.Replace(parts[i], wordToLookFor, wordMap.GetTranslation(nextWord));
                    }
                }
            }
            return parts;
        }

        private static bool CanEvaluateWordMap(WordMap wordMap, int partIdx)
        {
            return wordMap.Word != "Is" || partIdx == 0;
        }

        public static List<string> TryInsertTheWord(this List<string> parts, Action<List<string>> customInsertCallback = null)
        {
            if (customInsertCallback != null)
            {
                customInsertCallback.Invoke(parts);
            }
            else if (parts.Count > 0)
            {
                var checkWord = parts[0].GetWordFirstPart();
                var skipThe = checkWord.IsVerb();
                var addTheAnyway = Vsix2022.Constants.ADD_THE_ANYWAY_LIST.Any(w => w.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
                if (!skipThe || addTheAnyway)
                {
                    if (!parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                    {
                        parts.Insert(0, "The");
                    }
                    else
                    {
                        parts[0] = "The"; //force casing
                    }
                }
            }
            return parts;
        }

        public static List<string> AddPropertyBooleanPart(this List<string> parts)
        {
            if (parts.Count > 0)
            {
                var booleanPart = " a value indicating whether to";
                if (parts[0].IsPastTense() || parts[0].IsVerb())
                {
                    booleanPart = "a value indicating whether";
                }

                //is messes up readability. Lets remove it. ex) IsEnabledForDays
                var isTwoLettweWord = parts[0].IsTwoLetterPropertyExclusionVerb();//we only care if forst word is relavent
                if (isTwoLettweWord)
                {
                    parts.Remove(parts[0]);
                }
                parts.Insert(0, booleanPart);
            }

            return parts;
        }

        public static List<string> HandleAsyncKeyword(this List<string> parts, IOptionsService optionsService)
        {

            if (optionsService.ExcludeAsyncSuffix && parts.Last().IndexOf("async", StringComparison.OrdinalIgnoreCase) > -1)
            {
                parts.Remove(parts.Last());
            }
            var idx = parts.FindIndex(f => f.Equals("async", StringComparison.OrdinalIgnoreCase));
            if (idx > -1)
            {
                parts[idx] = "asynchronously";
            }
            return parts;
        }

        public static List<string> TryAddTodoSummary(this List<string> parts, string returnType, IOptionsService optionsService)
        {
            if (returnType == "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                if (optionsService.UseToDoCommentsOnSummaryError)
                {
                    parts = new List<string> { "TODO: Add Summary" };
                }
                else
                {
                    parts = new List<string>();
                }
            }
            return parts;
        }

        public static List<string> TryIncudeReturnType(this List<string> parts, IOptionsService optionsService, TypeSyntax returnType, Action<string> returnTapAction = null)
        {
            if (returnType.ToString() != "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                //if return type is not a generic type then just force the Todo comment cause what ever we do here will not be a good summary anyway
                if (returnType.GetType() != typeof(GenericNameSyntax))
                {
                    if (optionsService.UseToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { "TODO: Add Summary" };
                    }
                    else
                    {
                        parts.Clear();
                    }
                    return parts;
                }

                var options = new ReturnTypeBuilderOptions
                {
                    //ReturnBuildType = ReturnBuildType.SummaryXmlElement,
                    UseProperCasing = false,
                    TryToIncludeCrefsForReturnTypes = optionsService.TryToIncludeCrefsForReturnTypes,
                    IncludeStartingWordInText = true
                };
                var returnComment = new SingleWordCommentSummaryConstruction(returnType, options, optionsService).Comment;
                returnTapAction?.Invoke(returnComment);
                if (!string.IsNullOrEmpty(returnComment))
                {
                    if (!returnComment.StartsWith_A_An_And())
                    {
                        parts.Insert(1, "the");
                        parts.Insert(2, returnComment);
                    }
                    else
                    {
                        parts.Insert(1, returnComment);
                    }
                }
                else
                {
                    if (optionsService.UseToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { "TODO: Add Summary" };
                    }
                }
            }
            else
            {
                returnTapAction?.Invoke(null);
            }
            return parts;
        }

        public static List<string> TryPluarizeFirstWord(this List<string> parts)
        {
            if (parts.Count > 0)
            {
                if (parts.Count >= 2)
                {
                    parts[0] = Pluralizer.Pluralize(parts[0], parts[1]);
                }
                else
                {
                    parts[0] = Pluralizer.Pluralize(parts[0]);
                }
            }
            return parts;
        }
    }
}
