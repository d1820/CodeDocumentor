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
    /// <summary>
    /// The comment helper.
    /// </summary>
    public static class CommentHelper
    {
        /// <summary>
        /// Withs the period.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A string.</returns>
        public static string WithPeriod(this string text)
        {
            if (text.EndsWith("."))
            {
                return text;
            }
            return text + ".";
        }

        /// <summary>
        /// Creates the class comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public static string CreateClassComment(string name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        /// Create the record comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public static string CreateRecordComment(string name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        /// Creates the field comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public static string CreateFieldComment(string name)
        {
            List<string> parts = SpilitNameAndToLower(name, false, false);
            if (parts.Count == 1)
            {
                return $"The {string.Join(" ", parts.Select(s => s.ToLowerInvariant()))}.";
            }
            else
            {
                //At this point we have already pluralized and converted
                var skipThe = parts[0].IsVerbCombo();
                if (!skipThe)
                {
                    return $"The {string.Join(" ", parts.Select(s => s.ToLowerInvariant()))}.";
                }
                else
                {
                    if (parts.Count >= 2)
                    {
                        parts[0] = Pluralizer.Pluralize(parts[0], parts[1]);
                    }
                    else
                    {
                        parts[0] = Pluralizer.Pluralize(parts[0]);
                    }
                    return $"{string.Join(" ", parts)}.";
                }
            }


        }

        /// <summary>
        /// Creates the constructor comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isPrivate">If true, is private.</param>
        /// <returns>A string.</returns>
        public static string CreateConstructorComment(string name, bool isPrivate)
        {
            if (isPrivate)
            {
                return $"Prevents a default instance of the <see cref=\"{name}\"/> class from being created.".Translate();
            }
            else
            {
                return $"Initializes a new instance of the <see cref=\"{name}\"/> class.".Translate();
            }
        }

        /// <summary>
        /// Creates the interface comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public static string CreateInterfaceComment(string name)
        {
            List<string> parts = SpilitNameAndToLower(name, false);
            if (parts[0] == "I")
            {
                parts.RemoveAt(0);
            }

            parts.Insert(0, "The");
            parts.Add("interface");
            return string.Join(" ", parts).WithPeriod().Translate();
        }

        /// <summary>
        /// Creates the enum comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public static string CreateEnumComment(string name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        /// Creates the property comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isBoolean">If true, is boolean.</param>
        /// <param name="hasSetter">If true, has setter.</param>
        /// <returns>A string.</returns>
        public static string CreatePropertyComment(string name, bool isBoolean, bool hasSetter)
        {
            string comment = "Gets";
            if (hasSetter)
            {
                comment += " or Sets";
            }

            if (isBoolean)
            {
                comment += CreatePropertyBooleanPart(name).Translate();
            }
            else
            {
                comment += " the " + string.Join(" ", SpilitNameAndToLower(name, true));
            }
            return comment.WithPeriod();
        }

        /// <summary>
        /// Creates the method comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>A string.</returns>
        public static string CreateMethodComment(string name, TypeSyntax returnType)
        {
            List<string> parts = SpilitNameAndToLower(name, false, false);
            var isBool2part = parts.Count == 2 && returnType.IsBoolReturnType();
            if (parts.Count >= 2)
            {
                parts[0] = Pluralizer.PluralizeCustom(Pluralizer.Pluralize(parts[0], parts[1]), parts[1]);
            }
            else
            {
                parts[0] = Pluralizer.PluralizeCustom(Pluralizer.Pluralize(parts[0]));
            }

            if (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously"))
            {
                parts.Insert(1, "the");

                var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
                //try and use the return type for the value;
                if (returnType.ToString() != "void")
                {
                    string returnComment = new SingleWordMethodCommentConstruction(returnType).Comment;
                    if (!string.IsNullOrEmpty(returnComment))
                    {
                        parts.Insert(2, returnComment);
                    }
                    else
                    {
                        if (optionsService.UseToDoCommentsOnSummaryError)
                        {
                            return "TODO: Add Summary";
                        }
                        else
                        {
                            return returnComment;
                        }
                    }
                }
                else
                {
                    if (optionsService.UseToDoCommentsOnSummaryError)
                    {
                        return "TODO: Add Summary";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            else
            {
                //At this point we have already pluralized and converted
                var skipThe = parts[0].IsVerbCombo();
                var addTheAnyway = Constants.ADD_THE_ANYWAY_LIST.Any(w => w.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
                if ((!skipThe && !isBool2part) || addTheAnyway)
                {
                    parts.Insert(1, "the");
                }
            }

            return string.Join(" ", parts).Translate().WithPeriod();
        }

        /// <summary>
        /// Creates the parameter comment.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>A string.</returns>
        public static string CreateParameterComment(ParameterSyntax parameter)
        {
            bool isBoolean = false;
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
                return "If true, " + string.Join(" ", SpilitNameAndToLower(parameter.Identifier.ValueText, true)).WithPeriod();
            }
            else
            {
                return CreateCommonComment(parameter.Identifier.ValueText);
            }
        }

        /// <summary>
        /// Has comment.
        /// </summary>
        /// <param name="commentTriviaSyntax">The comment trivia syntax.</param>
        /// <returns>A bool.</returns>
        public static bool HasComment(DocumentationCommentTriviaSyntax commentTriviaSyntax)
        {
            bool hasSummary = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlElementSyntax>()
                .Any(o => o.StartTag.Name.ToString().Equals(DocumentationHeaderHelper.SUMMARY));

            bool hasInheritDoc = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlEmptyElementSyntax>()
                .Any(o => o.Name.ToString().Equals(DocumentationHeaderHelper.INHERITDOC));

            return hasSummary || hasInheritDoc;
        }

        /// <summary>
        /// Creates the property boolean part.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        private static string CreatePropertyBooleanPart(string name)
        {
            string booleanPart = " a value indicating whether ";

            var parts = SpilitNameAndToLower(name, true).ToList();

            string isWord = parts.FirstOrDefault(o => o == "is");
            if (isWord != null)
            {
                parts.Remove(isWord);
                parts.Insert(parts.Count - 1, isWord);
            }

            booleanPart += string.Join(" ", parts);
            return booleanPart;
        }

        /// <summary>
        /// Creates the common comment.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        private static string CreateCommonComment(string name)
        {
            return $"The {string.Join(" ", SpilitNameAndToLower(name, true))}.";
        }

        /// <summary>
        /// Spilit the name and to lower.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isFirstCharacterLower">If true, is first character lower.</param>
        /// <param name="shouldTranslate">If true, should translate.</param>
        /// <returns><![CDATA[List<string>]]></returns>
        internal static List<string> SpilitNameAndToLower(string name, bool isFirstCharacterLower, bool shouldTranslate = true)
        {
            if (shouldTranslate)
            {
                name = name.Translate();
            }
            List<string> parts = NameSplitter.Split(name);

            int i = isFirstCharacterLower ? 0 : 1;
            for (; i < parts.Count; i++)
            {
                if (!parts[i].All(a => char.IsUpper(a)))
                {
                    parts[i] = parts[i].ToLower();
                }
            }
            HandleAsyncKeyword(parts);
            return parts;
        }

        /// <summary>
        /// Handle asynchronously keyword.
        /// </summary>
        /// <param name="parts">The parts.</param>
        private static void HandleAsyncKeyword(List<string> parts)
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
        }
    }
}
