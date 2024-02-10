using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public abstract class BaseReturnTypeCommentConstruction
    {
        protected readonly bool UseProperCasing;

        /// <summary>
        /// Gets or Sets the array comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string ArrayCommentTemplate { get; }

        /// <summary>
        /// Generates the comment.
        /// </summary>
        public string Comment { get; protected set; }

        /// <summary>
        /// Gets or Sets the dictionary comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string DictionaryCommentTemplate { get;  }

        /// <summary>
        /// Gets or Sets the list comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string ListCommentTemplate { get;  }

        /// <summary>
        /// Gets or Sets the read only collection comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string ReadOnlyCollectionCommentTemplate { get;  }

        protected BaseReturnTypeCommentConstruction(bool useProperCasing)
        {
            UseProperCasing = useProperCasing;
        }

        /// <summary>
        /// Builds a comment
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="returnGenericTypeAsFullString">
        /// Flag indicating if the full type should just be returned as a string
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
            if (returnType is PredefinedTypeSyntax)
            {
                return GeneratePredefinedTypeComment(returnType as PredefinedTypeSyntax);
            }
            return GenerateGeneralComment(returnType.ToFullString().AsSpan());
        }

        /// <summary>
        /// Generates identifier name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        internal virtual string GenerateIdentifierNameTypeComment(IdentifierNameSyntax returnType)
        {
            return GenerateGeneralComment(returnType.Identifier.ValueText.AsSpan());
        }

        /// <summary>
        /// Generates qualified name type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        internal virtual string GenerateQualifiedNameTypeComment(QualifiedNameSyntax returnType)
        {
            return GenerateGeneralComment(returnType.ToString().AsSpan());
        }

        /// <summary>
        /// Generates predefined type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private string GeneratePredefinedTypeComment(PredefinedTypeSyntax returnType)
        {
            return GenerateGeneralComment(returnType.Keyword.ValueText.AsSpan());
        }

        /// <summary>
        /// Finds the parent MethodDeclarationSyntax if exists
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> </returns>
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax(SyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax) && node.Parent != null)
            {
                return GetMethodDeclarationSyntax(node.Parent);
            }
            return node as MethodDeclarationSyntax;
        }

        /// <summary>
        /// Builds the children generic arg list.
        /// </summary>
        /// <param name="argType"> The arg type. </param>
        /// <param name="items"> The items. </param>
        /// <param name="pluaralizeName"> If true, pluaralize name. </param>
        private void BuildChildrenGenericArgList(TypeSyntax argType, List<string> items, bool pluaralizeName = false, bool pluaralizeIdentifierType = true)
        {
            bool shouldPluralize;
            if (argType is GenericNameSyntax genericArgType)
            {
                var childArg = genericArgType.TypeArgumentList?.Arguments.FirstOrDefault();
                if (childArg != null)
                {
                    //we check the parent to see if the child needs to be pluralized
                    shouldPluralize = ShouldPluralize(argType, pluaralizeName);
                    BuildChildrenGenericArgList(childArg, items, shouldPluralize);
                }
            }
            items.Add(DetermineSpecificObjectName(argType, pluaralizeName, pluaralizeIdentifierType));
        }

        /// <summary>
        /// Determines specific object name.
        /// </summary>
        /// <param name="specificType"> The specific type. </param>
        /// <param name="pluaralizeName"> Flag determines if name should be pluralized </param>
        /// <returns> The comment. </returns>
        private string DetermineSpecificObjectName(TypeSyntax specificType, bool pluaralizeName = false, bool pluaralizeIdentifierType = true)
        {
            string value;
            string result;
            if (specificType is IdentifierNameSyntax identifierNameSyntax)
            {
                value = identifierNameSyntax.Identifier.ValueText.ApplyUserTranslations();
                result = pluaralizeIdentifierType? Pluralizer.Pluralize(value): value;
            }
            else if (specificType is PredefinedTypeSyntax predefinedTypeSyntax)
            {
                value = predefinedTypeSyntax.Keyword.ValueText.ApplyUserTranslations();
                result = pluaralizeName ? Pluralizer.Pluralize(value) : value;
            }
            else if (specificType is GenericNameSyntax genericNameSyntax)
            {
                value = genericNameSyntax.Identifier.ValueText.ApplyUserTranslations();

                result = pluaralizeName ? Pluralizer.Pluralize(value) : value;
            }
            else
            {
                result = specificType.ToFullString().ApplyUserTranslations();
            }
            return result;
        }

        /// <summary>
        /// Generates array type comment.
        /// </summary>
        /// <param name="arrayTypeSyntax"> The array type syntax. </param>
        /// <returns> The comment. </returns>
        private string GenerateArrayTypeComment(ArrayTypeSyntax arrayTypeSyntax)
        {
            return string.Format(ArrayCommentTemplate, DetermineSpecificObjectName(arrayTypeSyntax.ElementType, true));
        }

        /// <summary>
        /// Generates general comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private string GenerateGeneralComment(ReadOnlySpan<char> returnType)
        {
            //TODO: change the whole comment flow to support cref
            //We dont lowercase here cause its probably a type ie) Span, Custom<T>
            //var cref = $"<see cref=\"{returnType.ToString()}\"/>";
            //return cref;
            return returnType.ToString();
        }

        /// <summary>
        /// Generates generic type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The string </returns>
        private string GenerateGenericTypeComment(GenericNameSyntax returnType, bool returnGenericTypeAsFullString)
        {
            // this will return the full generic Ex. Task<Request>- which then will get added to a CDATA
            if (returnGenericTypeAsFullString)
            {
                return returnType.ToString();
            }

            var genericTypeStr = returnType.Identifier.ValueText;
            if (returnType.IsReadOnlyCollection())
            {
                var argType = returnType.TypeArgumentList.Arguments.First();
                var items = new List<string>();
                BuildChildrenGenericArgList(argType, items, true);
                items.Reverse();

                var resultStr = string.Join(" of ", items).ToLowerInvariant();
                return string.Format(ReadOnlyCollectionCommentTemplate, resultStr);
            }

            // IEnumerable IList List
            if (returnType.IsList())
            {
                var argType = returnType.TypeArgumentList.Arguments.First();
                var items = new List<string>();
                BuildChildrenGenericArgList(argType, items, true);
                items.Reverse();
                var resultStr = string.Join(" of ", items).ToLowerInvariant();
                return string.Format(ListCommentTemplate, resultStr);
            }

            if (returnType.IsDictionary())
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    var argType1 = returnType.TypeArgumentList.Arguments.First();
                    var argType2 = returnType.TypeArgumentList.Arguments.Last();
                    var items = new List<string>();
                    BuildChildrenGenericArgList(argType2, items, pluaralizeIdentifierType: false);
                    items.Reverse();
                    var resultStr = string.Join(" of ", items).ToLowerInvariant();
                    return string.Format(DictionaryCommentTemplate, argType1.ApplyUserTranslations(), resultStr);
                }
                return GenerateGeneralComment(genericTypeStr.AsSpan());
            }

            if (returnType.IsTask() || returnType.IsGenericActionResult())
            {
                var prefix = "<see cref=\"Task\"/> of type ";
                if (returnType.IsGenericActionResult())
                {
                    prefix += "<see cref=\"ActionResult\"/> of type ";
                }
                if (returnType.TypeArgumentList.Arguments.Count == 1)
                {
                    var firstType = returnType.TypeArgumentList.Arguments.First();
                    //List<string> items = new List<string>();
                    //BuildChildrenGenericArgList(firstType, items, false, false);
                    //TODO: fix comments flow to support cref nodes
                    var test = prefix + BuildComment(firstType, returnGenericTypeAsFullString);

                    return BuildComment(firstType, returnGenericTypeAsFullString);
                }
                //This should be impossible, but will handle just in case
                var builder = new StringBuilder();
                for (var i = 0; i < returnType.TypeArgumentList.Arguments.Count; i++)
                {
                    var item = returnType.TypeArgumentList.Arguments[i];
                    if (i > 0)
                    {
                        builder.Append($"{DocumentationHeaderHelper.DetermineStartingWord(item.ToString().AsSpan(), UseProperCasing)}");
                    }
                    builder.Append($"{BuildComment(item, returnGenericTypeAsFullString)}");
                    if (i + 1 < returnType.TypeArgumentList.Arguments.Count)
                    {
                        builder.Append(" and ");
                    }
                }

                return builder.ToString();
            }

            return GenerateGeneralComment(genericTypeStr.AsSpan());
        }

        /// <summary>
        /// Shoulds the pluralize.
        /// </summary>
        /// <param name="argType"> The arg type. </param>
        /// <param name="defaultValue"> If true, default value. </param>
        /// <returns> A bool. </returns>
        private bool ShouldPluralize(TypeSyntax argType, bool defaultValue)
        {
            if (argType.IsList() || argType.IsReadOnlyCollection())
            {
                return true;
            }
            return defaultValue;
        }
    }
}
