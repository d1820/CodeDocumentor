using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public class SingleWorkMethodCommentConstruction
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ReturnCommentConstruction" /> class.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        public SingleWorkMethodCommentConstruction(TypeSyntax returnType)
        {
            this.Comment = BuildComment(returnType);
        }
        /// <summary>
        /// Finds the parent MethodDeclarationSyntax if exists
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax(SyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax) && node.Parent != null)
            {
                return GetMethodDeclarationSyntax(node.Parent);
            }
            return node as MethodDeclarationSyntax;
        }

        /// <summary>
        /// Builds a string comment for a summary node
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private static string BuildComment(TypeSyntax returnType)
        {
            if (returnType is IdentifierNameSyntax identifier)
            {
                var parent = GetMethodDeclarationSyntax(returnType);
                if (parent != null && parent.TypeParameterList?.Parameters.Any(a => a.Identifier.ValueText == identifier.Identifier.ValueText) == true)
                {
                    var typeParamNode = DocumentationHeaderHelper.CreateTypeParameterRefElementSyntax(identifier.Identifier.ValueText);
                    return typeParamNode.ToFullString();
                }
                return GenerateIdentifierNameTypeComment(identifier);
            }
            else if (returnType is QualifiedNameSyntax)
            {
                return GenerateQualifiedNameTypeComment(returnType as QualifiedNameSyntax);
            }
            else if (returnType is GenericNameSyntax)
            {
                return GenerateGenericTypeComment(returnType as GenericNameSyntax);
            }
            else if (returnType is ArrayTypeSyntax)
            {
                return GenerateArrayTypeComment(returnType as ArrayTypeSyntax);
            }
            else
            {
                return GenerateGeneralComment(returnType.ToFullString());
            }
        }

        /// <summary>
        ///   Generates the comment.
        /// </summary>
        public string Comment { get; }


        /// <summary>
        ///   Generates identifier name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private static string GenerateIdentifierNameTypeComment(IdentifierNameSyntax returnType)
        {
            return $"<see cref=\"{returnType.Identifier.ValueText}\"/>";
            //return GenerateGeneralComment(returnType.Identifier.ValueText);
        }

        /// <summary>
        ///   Generates qualified name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private static string GenerateQualifiedNameTypeComment(QualifiedNameSyntax returnType)
        {
            return $"<see cref=\"{returnType.ToString()}\"/>";
            //return GenerateGeneralComment(returnType.ToString());
        }

        /// <summary>
        ///   Generates array type comment.
        /// </summary>
        /// <param name="arrayTypeSyntax"> The array type syntax. </param>
        /// <returns> The comment. </returns>
        private static string GenerateArrayTypeComment(ArrayTypeSyntax arrayTypeSyntax)
        {
            return "an array of " + DetermineSpecificObjectName(arrayTypeSyntax.ElementType);
        }

        /// <summary>
        ///   Generates generic type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private static string GenerateGenericTypeComment(GenericNameSyntax returnType)
        {
            string genericTypeStr = returnType.Identifier.ValueText;
            if (genericTypeStr.Contains("ReadOnlyCollection"))
            {
                return "read only collection of " + DetermineSpecificObjectName(returnType.TypeArgumentList.Arguments.First());
            }

            // IEnumerable IList List
            if (genericTypeStr == "IEnumerable" || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection"))
            {
                return "list of " + DetermineSpecificObjectName(returnType.TypeArgumentList.Arguments.First());
            }

            if (genericTypeStr.Contains("Dictionary"))
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    return $"dictionary with a key of type {returnType.TypeArgumentList.Arguments.FirstOrDefault()} and a value of type {returnType.TypeArgumentList.Arguments.LastOrDefault()}";
                }
                return GenerateGeneralComment(genericTypeStr);
            }

            if (genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && returnType.TypeArgumentList?.Arguments.Any() == true)
            {
                if (returnType.TypeArgumentList.Arguments.Count == 1)
                {
                    var firstType = returnType.TypeArgumentList.Arguments.First();
                    return BuildComment(firstType);
                }


                var builder = new StringBuilder();
                for (var i = 0; i < returnType.TypeArgumentList.Arguments.Count; i++)
                {
                    var item = returnType.TypeArgumentList.Arguments[i];
                    if (i > 0)
                    {
                        builder.Append($"{DetermineStartedWord(item.ToString()) } ");
                    }
                    builder.Append($"{BuildComment(item)}");
                    if (i + 1 < returnType.TypeArgumentList.Arguments.Count)
                    {
                        builder.Append(" and ");
                    }
                }

                return builder.ToString();
            }

            return GenerateGeneralComment(genericTypeStr);
        }

        //TODO: test this and implement. This would replace the above strings in GenerateGenericTypeComment, because that does not account for generics of generics
        private static string LookupNaturalStringByType(Type type)
        {
            if (type.IsAssignableFrom(typeof(IReadOnlyCollection<>)))
            {
                var baseStr = "read only collection of ";
                if (type.IsGenericType)
                {
                    foreach (var item in type.GenericTypeArguments)
                    {
                        baseStr += LookupNaturalStringByType(item);
                    }
                }
                return baseStr += type.Name;
            }

            if (type.IsAssignableFrom(typeof(IDictionary)))
            {
                if (type.GenericTypeArguments.Count() == 2)
                {
                    var key = LookupNaturalStringByType(type.GetGenericArguments().First());
                    var value = LookupNaturalStringByType(type.GetGenericArguments().Last());
                    return $"dictionary with a key of type {key} and a value of type {value}";
                }
            }
            if (type.IsAssignableFrom(typeof(IEnumerable)))
            {
                var baseStr = "list of ";
                if (type.IsGenericType)
                {
                    foreach (var item in type.GenericTypeArguments)
                    {
                        baseStr += LookupNaturalStringByType(item);
                    }
                }
                return baseStr += type.Name;
            }
            return type.Name;
        }

        /// <summary>
        ///   Generates general comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private static string GenerateGeneralComment(string returnType)
        {
            return returnType;
        }

        /// <summary>
        ///   Determines specific object name.
        /// </summary>
        /// <param name="specificType"> The specific type. </param>
        /// <returns> The comment. </returns>
        private static string DetermineSpecificObjectName(TypeSyntax specificType)
        {
            string result = null;
            if (specificType is IdentifierNameSyntax identifierNameSyntax)
            {
                result = Pluralizer.Pluralize((identifierNameSyntax).Identifier.ValueText);
            }
            else if (specificType is PredefinedTypeSyntax predefinedTypeSyntax)
            {
                result = (predefinedTypeSyntax).Keyword.ValueText;
            }
            else if (specificType is GenericNameSyntax genericNameSyntax)
            {
                result = (genericNameSyntax).Identifier.ValueText;
            }
            else
            {
                result = specificType.ToFullString();
            }
            return result + ".";
        }

        /// <summary>
        ///   Determines started word.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private static string DetermineStartedWord(string returnType)
        {
            var vowelChars = new List<char>() { 'a', 'e', 'i', 'o', 'u' };
            if (vowelChars.Contains(char.ToLower(returnType[0])))
            {
                return "an";
            }
            else
            {
                return "a";
            }
        }
    }
}
