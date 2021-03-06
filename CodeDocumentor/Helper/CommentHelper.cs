using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   The comment helper.
    /// </summary>
    public static class CommentHelper
    {
        /// <summary>
        ///   Adds period to text
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> </returns>
        public static string WithPeriod(this string text)
        {
            if (text.EndsWith("."))
            {
                return text;
            }
            return text + ".";
        }

        /// <summary>
        ///   Creates class comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The class comment. </returns>
        public static string CreateClassComment(ReadOnlySpan<char> name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        ///   Creates field comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The field comment. </returns>
        public static string CreateFieldComment(ReadOnlySpan<char> name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        ///   Creates constructor comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isPrivate"> If true, the constructor accessibility is private. </param>
        /// <returns> The contructor comment. </returns>
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
        ///   Creates the interface comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The class comment. </returns>
        public static string CreateInterfaceComment(ReadOnlySpan<char> name)
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
        ///   Creates the enum comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> A string. </returns>
        public static string CreateEnumComment(ReadOnlySpan<char> name)
        {
            return CreateCommonComment(name);
        }

        /// <summary>
        ///   Creates property comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isBoolean"> If ture, the property type is boolean. </param>
        /// <param name="hasSetter"> If ture, the property has setter. </param>
        /// <returns> The property comment. </returns>
        public static string CreatePropertyComment(ReadOnlySpan<char> name, bool isBoolean, bool hasSetter)
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
        ///   Creates method comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The method comment. </returns>
        public static string CreateMethodComment(ReadOnlySpan<char> name, TypeSyntax returnType)
        {
            List<string> parts = SpilitNameAndToLower(name, false, false);
            var isBool2part = parts.Count == 2 && returnType.ToString().IndexOf("bool", StringComparison.InvariantCultureIgnoreCase) > -1;
            if (!isBool2part)
            {
                parts[0] = Pluralizer.Pluralize(parts[0]);
            }
            if (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously"))
            {
                parts.Insert(1, "the");

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
                        if (CodeDocumentorPackage.Options.UseToDoCommentsOnSummaryError)
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
                    if (CodeDocumentorPackage.Options.UseToDoCommentsOnSummaryError)
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
                var skipThe = Constants.INTERNAL_SPECIAL_WORD_LIST.Any(w => w.Equals(parts[0]));
                if (!skipThe && !isBool2part)
                {
                    parts.Insert(1, "the");
                }
            }

            return string.Join(" ", parts).Translate().WithPeriod();
        }

        /// <summary>
        ///   Creates parameter comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The parameter comment. </returns>
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
                return "If true, " + string.Join(" ", SpilitNameAndToLower(parameter.Identifier.ValueText.AsSpan(), true)).WithPeriod();
            }
            else
            {
                return CreateCommonComment(parameter.Identifier.ValueText.AsSpan());
            }
        }

        /// <summary>
        ///   Have the comment.
        /// </summary>
        /// <param name="commentTriviaSyntax"> The comment trivia syntax. </param>
        /// <returns> A bool. </returns>
        public static bool HasComment(DocumentationCommentTriviaSyntax commentTriviaSyntax)
        {
            bool hasSummary = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlElementSyntax>()
                .Any(o => o.StartTag.Name.ToString().Equals(DocumentationHeaderHelper.Summary));

            bool hasInheritDoc = commentTriviaSyntax
                .ChildNodes()
                .OfType<XmlEmptyElementSyntax>()
                .Any(o => o.Name.ToString().Equals(DocumentationHeaderHelper.InheritDoc));

            return hasSummary || hasInheritDoc;
        }

        /// <summary>
        ///   Creates property boolean part.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The property comment boolean part. </returns>
        private static string CreatePropertyBooleanPart(ReadOnlySpan<char> name)
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
        ///   Creates common comment.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The common comment. </returns>
        private static string CreateCommonComment(ReadOnlySpan<char> name)
        {
            return $"The {string.Join(" ", SpilitNameAndToLower(name, true))}.";
        }

        /// <summary>
        ///   Splits name and make words lower.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="isFirstCharacterLower"> If true, the first character will be lower. </param>
        /// <param name="shouldTranslate">If true, the name will be translated</param>
        /// <returns> A list of words. </returns>
        private static List<string> SpilitNameAndToLower(ReadOnlySpan<char> name, bool isFirstCharacterLower, bool shouldTranslate = true)
        {
            if (shouldTranslate)
            {
                name = name.TranslateToSpan();
            }
            List<string> parts = NameSplitter.Split(name);

            int i = isFirstCharacterLower ? 0 : 1;
            for (; i < parts.Count; i++)
            {
                parts[i] = parts[i].ToLower();
            }
            HandleAsyncKeyword(parts);
            return parts;
        }

        /// <summary>
        ///   Updates or removes the async keyword
        /// </summary>
        /// <param name="parts"> The list of parts of the member name separated by uppercase letters </param>
        private static void HandleAsyncKeyword(List<string> parts)
        {
            if (CodeDocumentorPackage.Options.ExcludeAsyncSuffix && parts.Last().IndexOf("async", System.StringComparison.OrdinalIgnoreCase) > -1)
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
