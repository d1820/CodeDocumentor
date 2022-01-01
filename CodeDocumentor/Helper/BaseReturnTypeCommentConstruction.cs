using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;

namespace CodeDocumentor.Helper
{
    public abstract class BaseReturnTypeCommentConstruction
    {
        protected readonly bool _useProperCasing;
        protected readonly bool _useStartingWords;

        public abstract string ReadOnlyCollectionCommentTemplate { get; set; }

        public abstract string ListCommentTemplate { get; set; }

        public abstract string DictionaryCommentTemplate { get; set; }

        public abstract string ArrayCommentTemplate { get; set; }

        protected BaseReturnTypeCommentConstruction(bool useProperCasing, bool useStartingWords)
        {
            _useProperCasing = useProperCasing;
            _useStartingWords = useStartingWords;
        }

        /// <summary>
        ///   Generates the comment.
        /// </summary>
        public string Comment { get; protected set; }

        /// <summary>
        ///   Builds a comment
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="returnGenericTypeAsFullString">
        ///   Flag indicating if the full type should just be returned as a string
        /// </param>
        /// <returns> The comment </returns>
        internal virtual string BuildComment(TypeSyntax returnType, bool returnGenericTypeAsFullString)
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

            if (returnType is QualifiedNameSyntax)
            {
                return GenerateQualifiedNameTypeComment(returnType as QualifiedNameSyntax);
            }
            if (returnType is GenericNameSyntax)
            {
                return GenerateGenericTypeComment(returnType as GenericNameSyntax, returnGenericTypeAsFullString);
            }
            if (returnType is ArrayTypeSyntax)
            {
                return GenerateArrayTypeComment(returnType as ArrayTypeSyntax);
            }

            return GenerateGeneralComment(returnType.ToFullString().AsSpan(), _useStartingWords).WithPeriod();
        }

        /// <summary>
        ///   Generates array type comment.
        /// </summary>
        /// <param name="arrayTypeSyntax"> The array type syntax. </param>
        /// <returns> The comment. </returns>
        protected string GenerateArrayTypeComment(ArrayTypeSyntax arrayTypeSyntax)
        {
            return string.Format(ArrayCommentTemplate, DetermineSpecificObjectName(arrayTypeSyntax.ElementType, true));
        }

        private void BuildChildrenGenericArgList(TypeSyntax argType, List<string> items, bool pluaralizeName = false)
        {
            if (argType is GenericNameSyntax genericArgType)
            {
                var childArg = genericArgType.TypeArgumentList?.Arguments.FirstOrDefault();
                if (childArg != null)
                {
                    var shouldPluralize = pluaralizeName;
                    var argTypeStr = argType.ToString();
                    if (argTypeStr == "IEnumerable" || argTypeStr.Contains("List") || argTypeStr.Contains("Collection") || argTypeStr.Contains("ReadOnlyCollection"))
                    {
                        shouldPluralize = true;
                    }

                    BuildChildrenGenericArgList(childArg, items, shouldPluralize);
                }
            }
            items.Add(DetermineSpecificObjectName(argType, pluaralizeName));
        }

        /// <summary>
        ///   Generates generic type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        protected string GenerateGenericTypeComment(GenericNameSyntax returnType, bool returnGenericTypeAsFullString)
        {
            // this will return the full generic Ex. Task<Request>- which then will get added to a CDATA
            if (returnGenericTypeAsFullString)
            {
                return returnType.ToFullString();
            }

            string genericTypeStr = returnType.Identifier.ValueText;
            if (genericTypeStr.Contains("ReadOnlyCollection"))
            {
                var argType = returnType.TypeArgumentList.Arguments.First();
                List<string> items = new List<string>();
                BuildChildrenGenericArgList(argType, items, true);
                items.Reverse();
                
                var resultStr = string.Join(ReadOnlyCollectionCommentTemplate, items).ToLowerInvariant();
                return string.Format(ReadOnlyCollectionCommentTemplate, resultStr);
            }

            // IEnumerable IList List
            if (genericTypeStr == "IEnumerable" || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection"))
            {
                var argType = returnType.TypeArgumentList.Arguments.First();
                List<string> items = new List<string>();
                BuildChildrenGenericArgList(argType, items, true);
                items.Reverse();
                var resultStr = string.Join(" of ", items).ToLowerInvariant();
                return string.Format(ListCommentTemplate, resultStr);
            }

            if (genericTypeStr.Contains("Dictionary"))
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    var argType1 = returnType.TypeArgumentList.Arguments.First();
                    var argType2 = returnType.TypeArgumentList.Arguments.Last();
                    List<string> items = new List<string>();
                    BuildChildrenGenericArgList(argType2, items);
                    items.Reverse();
                    var resultStr = string.Join(" of ", items).ToLowerInvariant();
                    return string.Format(DictionaryCommentTemplate, argType1, resultStr);
                }
                return GenerateGeneralComment(genericTypeStr.AsSpan(), _useStartingWords).WithPeriod();
            }

            if (genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && returnType.TypeArgumentList?.Arguments.Any() == true)
            {
                if (returnType.TypeArgumentList.Arguments.Count == 1)
                {
                    var firstType = returnType.TypeArgumentList.Arguments.First();
                    return BuildComment(firstType, returnGenericTypeAsFullString);
                }

                var builder = new StringBuilder();
                for (var i = 0; i < returnType.TypeArgumentList.Arguments.Count; i++)
                {
                    var item = returnType.TypeArgumentList.Arguments[i];
                    if (i > 0)
                    {
                        builder.Append($"{DetermineStartingWord(item.ToString().AsSpan()) }");
                    }
                    builder.Append($"{BuildComment(item, returnGenericTypeAsFullString)}");
                    if (i + 1 < returnType.TypeArgumentList.Arguments.Count)
                    {
                        builder.Append(" and ");
                    }
                }

                return builder.ToString();
            }

            return GenerateGeneralComment(genericTypeStr.AsSpan(), false).WithPeriod();
        }

        /// <summary>
        ///   Generates predefined type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        protected string GeneratePredefinedTypeComment(PredefinedTypeSyntax returnType)
        {
            return GenerateGeneralComment(returnType.Keyword.ValueText.AsSpan(), true).WithPeriod();
        }

        /// <summary>
        ///   Generates identifier name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        protected virtual string GenerateIdentifierNameTypeComment(IdentifierNameSyntax returnType)
        {
            return GenerateGeneralComment(returnType.Identifier.ValueText.AsSpan(), _useStartingWords).WithPeriod();
        }

        /// <summary>
        ///   Generates qualified name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        protected virtual string GenerateQualifiedNameTypeComment(QualifiedNameSyntax returnType)
        {
            return GenerateGeneralComment(returnType.ToString().AsSpan(), _useStartingWords).WithPeriod();
        }

        /// <summary>
        ///   Generates general comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="includeStartingWord"> Flag to determine if a starting word should be included </param>
        /// <returns> The comment. </returns>
        public string GenerateGeneralComment(ReadOnlySpan<char> returnType, bool includeStartingWord)
        {
            if (includeStartingWord)
            {
                return string.Format("{0} {1}", DetermineStartingWord(returnType), returnType.ToString());
            }
            return returnType.ToString();
        }

        /// <summary>
        ///   Determines specific object name.
        /// </summary>
        /// <param name="specificType"> The specific type. </param>
        /// <param name="pluaralizeName">Flag determines if name should be pluralized</param>
        /// <returns> The comment. </returns>
        protected string DetermineSpecificObjectName(TypeSyntax specificType, bool pluaralizeName = false)
        {
            string result = null;
            if (specificType is IdentifierNameSyntax identifierNameSyntax)
            {
                result = Pluralizer.Pluralize(identifierNameSyntax.Identifier.ValueText);
            }
            else if (specificType is PredefinedTypeSyntax predefinedTypeSyntax)
            {
                result = pluaralizeName ? Pluralizer.Pluralize(predefinedTypeSyntax.Keyword.ValueText) : predefinedTypeSyntax.Keyword.ValueText;
            }
            else if (specificType is GenericNameSyntax genericNameSyntax)
            {
                result = pluaralizeName ? Pluralizer.Pluralize(genericNameSyntax.Identifier.ValueText) : genericNameSyntax.Identifier.ValueText;
            }
            else
            {
                result = specificType.ToFullString();
            }
            return result;
        }

        /// <summary>
        ///   Determines started word.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        protected string DetermineStartingWord(ReadOnlySpan<char> returnType)
        {
            var vowelChars = new List<char>() { 'a', 'e', 'i', 'o', 'u' };
            if (vowelChars.Contains(char.ToLower(returnType[0])))
            {
                return _useProperCasing ? "An" : "an";
            }
            else
            {
                return _useProperCasing ? "A" : "a";
            }
        }

        /// <summary>
        ///   Finds the parent MethodDeclarationSyntax if exists
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> </returns>
        protected static MethodDeclarationSyntax GetMethodDeclarationSyntax(SyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax) && node.Parent != null)
            {
                return GetMethodDeclarationSyntax(node.Parent);
            }
            return node as MethodDeclarationSyntax;
        }

        //TODO: test this and implement. This would replace the above strings in GenerateGenericTypeComment, because that does not account for generics of generics
        private string LookupNaturalStringByType(Type type)
        {
            if (type.IsAssignableFrom(typeof(IReadOnlyCollection<>)))
            {
                var baseStr = "A read only collection of ";
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
                    return $"A dictionary with a key of type {key} and a value of type {value}";
                }
            }
            if (type.IsAssignableFrom(typeof(IEnumerable)))
            {
                var baseStr = "A list of ";
                if (type.IsGenericType)
                {
                    foreach (var item in type.GenericTypeArguments)
                    {
                        baseStr += LookupNaturalStringByType(item);
                    }
                }
                return baseStr += type.Name;
            }
            return DetermineStartingWord(type.Name.AsSpan());
        }
    }
}
