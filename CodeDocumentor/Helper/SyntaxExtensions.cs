using System;
using System.Linq;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public static class SyntaxExtensions
    {
        public static bool IsOwnedByInterface(this SyntaxNode node)
        {
            return node?.Parent.GetType() == typeof(InterfaceDeclarationSyntax);
        }

        /// <summary>
        ///  Translates text replacing words from the WordMap settings
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> A string </returns>
        public static string ApplyUserTranslations(this CSharpSyntaxNode node, WordMap[] wordMaps)
        {
            return node.ToString().ApplyUserTranslations(wordMaps);
        }

        /// <summary>
        ///  Checks if is dictionary.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsDictionary(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Dictionary");
        }

        public static bool IsDictionary(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Dictionary");
        }

        /// <summary>
        ///  Checks if is generic action result.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsGenericActionResult(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("ActionResult", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        public static bool IsGenericValueTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("ValueTask", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        /// <summary>
        ///  Checks if is list.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        public static bool PropertyHasSetter(this PropertyDeclarationSyntax declarationSyntax)
        {
            var hasSetter = false;

            if (declarationSyntax.AccessorList != null && declarationSyntax.AccessorList.Accessors.Any(o => o.Kind() == SyntaxKind.SetAccessorDeclaration))
            {
                if (declarationSyntax.AccessorList.Accessors.First(o => o.Kind() == SyntaxKind.SetAccessorDeclaration).ChildTokens().Any(o => o.IsKind(SyntaxKind.PrivateKeyword) || o.IsKind(SyntaxKind.InternalKeyword)))
                {
                    // private set or internal set should consider as no set.
                    hasSetter = false;
                }
                else
                {
                    hasSetter = true;
                }
            }

            return hasSetter;
        }

        /// <summary>
        ///  Checks if is list.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        public static bool IsPropertyReturnTypeBool(this PropertyDeclarationSyntax declarationSyntax)
        {
            var isBoolean = false;
            if (declarationSyntax.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = ((PredefinedTypeSyntax)declarationSyntax.Type).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (declarationSyntax.Type.IsKind(SyntaxKind.NullableType))
            {
                if (((NullableTypeSyntax)declarationSyntax.Type).ElementType is PredefinedTypeSyntax returnType)
                {
                    isBoolean = returnType.ToString().IndexOf("bool", StringComparison.OrdinalIgnoreCase) > -1;
                }
            }

            return isBoolean;
        }

        /// <summary>
        ///  Checks if is read only collection.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this GenericNameSyntax nameSyntax)
        {
            return nameSyntax.Identifier.ValueText.Contains("ReadOnlyCollection");
        }

        /// <summary>
        ///  Checks if is read only collection.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this TypeSyntax nameSyntax)
        {
            return nameSyntax.ToString().Contains("ReadOnlyCollection");
        }

        /// <summary>
        ///  Checks if is task.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }
        /// <summary>
        ///  Has summary.
        /// </summary>
        /// <param name="syntax"> The syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool HasSummary(this CSharpSyntaxNode syntax)
        {
            return syntax.HasLeadingTrivia && syntax.GetLeadingTrivia().Any(a => a.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia));
        }

        /// <summary>
        ///  Checks if is private.
        /// </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool IsPrivate(this BaseMethodDeclarationSyntax declarationSyntax)
        {
            var isPrivate = false;
            if (declarationSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                isPrivate = true;
            }
            return isPrivate;
        }

        /// <summary>
        ///  Gets the element syntax.
        /// </summary>
        /// <param name="syntax"> The syntax. </param>
        /// <param name="name"> The name. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax GetElementSyntax(this CSharpSyntaxNode syntax, string name)
        {
            if (syntax.HasLeadingTrivia)
            {
                var docComment = syntax.GetLeadingTrivia().FirstOrDefault(a => a.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                                                                || a.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                                || a.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia));
                if (docComment != default)
                {
                    var docTriviaSyntax = docComment.GetStructure() as DocumentationCommentTriviaSyntax;
                    var items = docTriviaSyntax?.Content
                        .OfType<XmlElementSyntax>();

                    var match = items
                        .FirstOrDefault(element => string.Equals(element.StartTag.Name.LocalName.ValueText, name, StringComparison.OrdinalIgnoreCase));

                    return match;
                }
            }
            return null;
        }


        /// <summary>
        ///  Upserts the leading trivia.
        /// </summary>
        /// <param name="leadingTrivia"> The leading trivia. </param>
        /// <param name="commentTrivia"> The comment trivia. </param>
        /// <returns> A SyntaxTriviaList. </returns>
        internal static SyntaxTriviaList UpsertLeadingTrivia(this SyntaxTriviaList leadingTrivia, DocumentationCommentTriviaSyntax commentTrivia)
        {
            if (leadingTrivia.All(a => a.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                return leadingTrivia.Add(SyntaxFactory.Trivia(commentTrivia));
            }

            var existingIndex = leadingTrivia.Select((node, index) => new { node, index }).FirstOrDefault(
                        f =>
                         f.node.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                            || f.node.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                            || f.node.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                        )?.index ?? -1;
            return existingIndex < 0
                ? leadingTrivia.Insert(leadingTrivia.Count - 1, SyntaxFactory.Trivia(commentTrivia))
                : leadingTrivia.Replace(leadingTrivia[existingIndex], SyntaxFactory.Trivia(commentTrivia));
        }
    }
}
