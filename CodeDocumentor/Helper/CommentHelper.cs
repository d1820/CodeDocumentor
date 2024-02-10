using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("CodeDocumentor.Test")]

namespace CodeDocumentor.Helper
{
    /// <summary> The comment helper. </summary>
    public static class CommentHelper
    {
        /// <summary> Creates the class comment. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateClassComment(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts()
                         .TryPluarizeFirstWord()
                         .TryInsertTheWord()
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations()
                         .WithPeriod();
            //return CreateCommonComment(name).ApplyUserTranslations().WithPeriod();
            return comment;
        }

        /// <summary> Creates the constructor comment. </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isPrivate"> If true, is private. </param>
        /// <returns> A string. </returns>
        public static string CreateConstructorComment(string name, bool isPrivate)
        {
            string comment;
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            if (isPrivate)
            {
                comment = $"Prevents a default instance of the <see cref=\"{name}\"/> class from being created";
            }
            else
            {
                comment = $"Initializes a new instance of the <see cref=\"{name}\"/> class";
            }
            return comment.ApplyUserTranslations().WithPeriod();
        }

        /// <summary> Creates the enum comment. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateEnumComment(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                           .Split(name)
                           .TranslateParts()
                           .TryPluarizeFirstWord()
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .PluaralizeLastWord()
                           .JoinToString()
                           .ApplyUserTranslations()
                           .WithPeriod();
            //CreateCommonComment(name).ApplyUserTranslations().WithPeriod()
            return comment;
        }

        /// <summary> Creates the field comment. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateFieldComment(string name)
        {
            //string comment;
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            //order matters. fields are special in the sense there is not action attached and we dont need to do translations
            var comment = NameSplitter
                            .Split(name)
                            .HandleAsyncKeyword()
                            .Tap((parts) => {
                                if (char.IsLower(parts[0], 0)) //if first letter of a field is lower its prob a private field. Lets adjust for it
                                {
                                    parts[0] = parts[0].ToTitleCase();
                                }
                            })
                            .TryInsertTheWord()
                            .ToLowerParts()
                            .JoinToString()
                            .ApplyUserTranslations()
                            .WithPeriod();




            //if (parts.Count >= 2)
            //{
            //    parts[0] = Pluralizer.Pluralize(parts[0], parts[1]);
            //}
            //else
            //{
            //    parts[0] = Pluralizer.Pluralize(parts[0]);
            //}

            //var skipThe = parts[0].IsVerb();
            //if (parts.Count == 1)
            //{
            //    if (!skipThe)
            //    {
            //        comment = $"The {string.Join(" ", parts.Select(s => s.ToLowerInvariant()))}";
            //    }
            //    else
            //    {
            //        comment = string.Join(" ", parts.Select(s => s.ToLowerInvariant()));
            //    }
            //}
            //else
            //{
            //    if (!skipThe)
            //    {
            //        comment = $"The {string.Join(" ", parts.Select(s => s.ToLowerInvariant()))}";
            //    }
            //    else
            //    {

            //        comment = $"{string.Join(" ", parts)}";
            //    }
            //}
            return comment;
        }

        /// <summary> Creates the interface comment. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateInterfaceComment(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                           .Split(name)
                           .Tap((parts) => {
                               parts.Remove("I");
                           })
                           .AddCustomPart("interface")
                           .TranslateParts()
                           .TryPluarizeFirstWord()
                           .TryInsertTheWord()
                           .ToLowerParts()
                           .JoinToString()
                           .ApplyUserTranslations()
                           .WithPeriod();

            return comment;

            //var parts = SpilitNameAndToLower(name);
            //if (parts[0] == "I")
            //{
            //    parts.RemoveAt(0);
            //}

            //parts.Insert(0, "The");
            //parts.Add("interface");
            //return string.Join(" ", parts).ApplyUserTranslations().WithPeriod();
        }

        /// <summary> Creates the method comment. </summary>
        /// <param name="name"> The name. </param>
        /// <param name="returnType"> The return type. </param>
        /// <returns> A string. </returns>
        public static string CreateMethodComment(string name, TypeSyntax returnType)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            //var parts = SpilitNameAndToLower(name);
            var isBool2part = false;
            var comment = NameSplitter
                         .Split(name)
                         .Tap((parts) =>
                         {
                             isBool2part = parts.Count == 2 && returnType.IsBoolReturnType();
                         })
                         .HandleAsyncKeyword()
                         .TryIncudeReturnType(returnType)
                         .TryAddTodoSummary(returnType.ToString())
                         .TranslateParts()
                         .TryPluarizeFirstWord()
                         .TryInsertTheWord((parts) =>
                         {
                             if (!isBool2part)
                             {
                                 parts.Insert(1, "the");
                             }
                         })
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations()
                         .WithPeriod();
            return comment;

            //var isBool2part = parts.Count == 2 && returnType.IsBoolReturnType();
            //if (parts.Count >= 2)
            //{
            //    parts[0] = Pluralizer.PluralizeCustom(Pluralizer.Pluralize(parts[0], parts[1]), parts[1]);
            //}
            //else
            //{
            //    parts[0] = Pluralizer.PluralizeCustom(Pluralizer.Pluralize(parts[0]));
            //}

            //if (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously"))
            //{
            //    var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            //    //try and use the return type for the value;
            //    if (returnType.ToString() != "void")
            //    {
            //        var returnComment = new SingleWordMethodCommentConstruction(returnType).Comment;

            //        if (!string.IsNullOrEmpty(returnComment))
            //        {
            //            if (!returnComment.StartsWith("a", StringComparison.InvariantCultureIgnoreCase) && !returnComment.StartsWith("an", StringComparison.InvariantCultureIgnoreCase))
            //            {
            //                parts.Insert(1, "the");
            //                parts.Insert(2, returnComment);
            //            }
            //            else
            //            {
            //                parts.Insert(1, returnComment);
            //            }
            //        }
            //        else
            //        {
            //            if (optionsService.UseToDoCommentsOnSummaryError)
            //            {
            //                return "TODO: Add Summary";
            //            }
            //            else
            //            {
            //                return returnComment.ApplyUserTranslations().WithPeriod();
            //            }
            //        }
            //    }
            //    else if (optionsService.UseToDoCommentsOnSummaryError)
            //    {
            //        return "TODO: Add Summary";
            //    }
            //    else
            //    {
            //        return string.Empty;
            //    }
            //}
            //else
            //{
            //    //At this point we have already pluralized and converted
            //    var skipThe = parts[0].IsVerb();
            //    var addTheAnyway = Constants.ADD_THE_ANYWAY_LIST.Any(w => w.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
            //    if ((!skipThe && !isBool2part) || addTheAnyway)
            //    {
            //        parts.Insert(1, "the");
            //    }
            //}

            //return string.Join(" ", parts).ApplyUserTranslations().WithPeriod();
        }

        /// <summary> Creates the parameter comment. </summary>
        /// <param name="parameter"> The parameter. </param>
        /// <returns> A string. </returns>
        public static string CreateParameterComment(ParameterSyntax parameter)
        {
            var isBoolean = false;
            if (parameter.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = (parameter.Type as PredefinedTypeSyntax).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (parameter.Type.IsKind(SyntaxKind.NullableType))
            {
                var type = (parameter.Type as NullableTypeSyntax).ElementType as PredefinedTypeSyntax;

                // If it is not predefined type syntax, it should be IdentifierNameSyntax.
                if (type != null)
                {
                    isBoolean = type.Keyword.IsKind(SyntaxKind.BoolKeyword);
                }
            }

            if (isBoolean)
            {
                var comment = NameSplitter
                                .Split(parameter.Identifier.ValueText)
                                .AddCustomPart("If true, ", 0)
                                .TranslateParts()
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations()
                                .WithPeriod();
                //return "If true, " + string.Join(" ", SpilitNameAndToLower(parameter.Identifier.ValueText, true)).WithPeriod();
                return comment;
            }
            else
            {
                var comment = NameSplitter
                                .Split(parameter.Identifier.ValueText)
                                .TranslateParts()
                                .TryPluarizeFirstWord()
                                .TryInsertTheWord()
                                .ToLowerParts()
                                .JoinToString()
                                .ApplyUserTranslations()
                                .WithPeriod();
                //return CreateCommonComment(parameter.Identifier.ValueText).WithPeriod();
                return comment;
            }
        }

        /// <summary> Creates the property comment. </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isBoolean"> If true, is boolean. </param>
        /// <param name="hasSetter"> If true, has setter. </param>
        /// <returns> A string. </returns>
        public static string CreatePropertyComment(string name, bool isBoolean, bool hasSetter)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            //var comment = "Gets";
            //if (hasSetter)
            //{
            //    comment += " or Sets";
            //}

            if (isBoolean)
            {
                var comment = NameSplitter
                              .Split(name)
                              .AddPropertyBooleanPart() //we do this here cause it will get pushed down the stack
                              .AddCustomPart("Gets", 0)
                              .AddCustomPart(hasSetter ? "or Sets" : null, 1)
                              .TranslateParts()
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations()
                              .WithPeriod();
                //comment += CreatePropertyBooleanPart(name).ApplyUserTranslations();
                return comment;
            }
            else
            {
                var comment = NameSplitter
                              .Split(name)
                              .AddCustomPart("the", 0) //we do this here cause it will get pushed down the stack
                              .AddCustomPart("Gets", 0)
                              .AddCustomPart(hasSetter ? "or Sets" : null, 1)
                              .TranslateParts()
                              .ToLowerParts()
                              .JoinToString()
                              .ApplyUserTranslations()
                              .WithPeriod();
                return comment;

                //comment += " the " + string.Join(" ", SpilitNameAndToLower(name, true));
            }
            //return comment.ApplyUserTranslations().WithPeriod();
        }

        /// <summary> Create the record comment. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateRecordComment(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            var comment = NameSplitter
                         .Split(name)
                         .TranslateParts()
                         .TryPluarizeFirstWord()
                         .TryInsertTheWord()
                         .ToLowerParts()
                         .JoinToString()
                         .ApplyUserTranslations()
                         .WithPeriod();
            //return CreateCommonComment(name).ApplyUserTranslations().WithPeriod();
            return comment;
        }

        /// <summary> Has comment. </summary>
        /// <param name="commentTriviaSyntax"> The comment trivia syntax. </param>
        /// <returns> A bool. </returns>
        public static bool HasComment(DocumentationCommentTriviaSyntax commentTriviaSyntax)
        {
            var hasSummary = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlElementSyntax>()
                .Any(o => o.StartTag.Name.ToString().Equals(DocumentationHeaderHelper.SUMMARY));

            var hasInheritDoc = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlEmptyElementSyntax>()
                .Any(o => o.Name.ToString().Equals(DocumentationHeaderHelper.INHERITDOC));

            return hasSummary || hasInheritDoc;
        }

        /// <summary> Withs the period. </summary>
        /// <param name="text"> The text. </param>
        /// <returns> A string. </returns>
        public static string WithPeriod(this string text)
        {
            if (text?.Trim().EndsWith(".") == true)
            {
                return text;
            }
            if (text.Length > 0)
            {
                return text + ".";
            }
            return text;
        }

        internal static List<string> ToLowerParts(this List<string> parts, bool forceFirstCharToLower = false)
        {
            var i = forceFirstCharToLower ||
                    (parts[0] != "The" && parts[0] != "If true, "
                        && !parts[0].IsVerb() //if the first word is a verb we are not adding The anyway so we need to leave it Pascal
                    )
                ? 0 : 1;
            for (; i < parts.Count; i++)
            {
                if (!parts[i].All(a => char.IsUpper(a)))
                {
                    parts[i] = parts[i].ToLower();
                }
            }
            //First letter is always caps unless it was forced lower
            if (!forceFirstCharToLower && char.IsLower(parts[0], 0))
            {
                parts[0] = parts[0].ToTitleCase();// char.ToUpper(parts[0][0]) + parts[0].Substring(1);
            }
            return parts;
        }

        internal static List<string> PluaralizeLastWord(this List<string> parts)
        {
            var lastIdx = parts.Count - 1;
            parts[lastIdx] = Pluralizer.ForcePluralization(parts[lastIdx]);
            return parts;
        }

        private static List<string> AddPropertyBooleanPart(this List<string> parts)
        {
            var booleanPart = " a value indicating whether to";
            if (parts[0].IsPastTense())
            {
                booleanPart = " a value indicating whether ";
            }
            //var booleanPart = " a value indicating whether to ";

            //var parts = SpilitNameAndToLower(name, true).ToList();

            //is messes up readability. Lets remove it. ex) IsEnabledForDays
            var isWord = parts.FirstOrDefault(o => o == "is");
            if (isWord != null)
            {
                parts.Remove(isWord);
                //parts.Insert(parts.Count - 1, isWord);
            }
            parts.Insert(0, booleanPart);

            //booleanPart += string.Join(" ", parts);
            return parts;
        }

        private static List<string> HandleAsyncKeyword(this List<string> parts)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();

            if (optionsService.ExcludeAsyncSuffix && parts.Last().IndexOf("async", System.StringComparison.OrdinalIgnoreCase) > -1)
            {
                parts.Remove(parts.Last());
            }
            var idx = parts.FindIndex(f => f.Equals("async", System.StringComparison.OrdinalIgnoreCase));
            if (idx > -1)
            {
                parts[idx] = "asynchronously";
            }
            return parts;
        }

        private static List<string> TryInsertTheWord(this List<string> parts, Action<List<string>> customInsertCallback = null)
        {
            var checkWord = parts[0].GetWordFirstPart();
            var skipThe = checkWord.IsVerb();
            var addTheAnyway = Constants.ADD_THE_ANYWAY_LIST.Any(w => w.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
            if (!skipThe || addTheAnyway)
            {
                if (customInsertCallback != null)
                {
                    customInsertCallback.Invoke(parts);
                }
                else
                {
                    parts.Insert(0, "The");
                }
            }
            return parts;
        }

        private static List<string> TryPluarizeFirstWord(this List<string> parts)
        {
            if (parts.Count >= 2)
            {
                parts[0] = Pluralizer.Pluralize(parts[0], parts[1]);
            }
            else
            {
                parts[0] = Pluralizer.Pluralize(parts[0]);
            }
            return parts;
        }

        private static string JoinToString(this List<string> parts, string delimiter = " ")
        {
            return $"{string.Join(delimiter, parts)}";
        }

        private static List<string> TranslateParts(this List<string> parts)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                parts[i] = p.InternalTranslateText(i);
            }
            return parts;
        }

        private static List<string> Tap(this List<string> parts, Action<List<string>> tapAction)
        {
            tapAction?.Invoke(parts);
            return parts;
        }

        private static List<string> TryIncudeReturnType(this List<string> parts, TypeSyntax returnType)
        {
            if (returnType.ToString() != "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
                var returnComment = new SingleWordMethodCommentConstruction(returnType).Comment;

                if (!string.IsNullOrEmpty(returnComment))
                {
                    if (!returnComment.StartsWith("a", StringComparison.InvariantCultureIgnoreCase) && !returnComment.StartsWith("an", StringComparison.InvariantCultureIgnoreCase))
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
            return parts;
        }

        private static List<string> TryAddTodoSummary(this List<string> parts, string returnType)
        {
            if (returnType == "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
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

        private static List<string> AddCustomPart(this List<string> parts, string part = null, int idx = -1)
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
            part.Insert(idx, part);
            return parts;
        }
    }
}
