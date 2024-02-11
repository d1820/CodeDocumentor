using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public class ReturnTypeBuilderOptions
    {
        public bool ForcePredefinedTypeEvaluation { get; set; }
        public bool ReturnGenericTypeAsFullString { get; set; }
        public bool IsRootReturnType { get; set; } = true;
        public bool UseProperCasing { get; set; }
    }
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
        public abstract string DictionaryCommentTemplate { get; }

        /// <summary>
        /// Gets or Sets the list comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string ListCommentTemplate { get; }

        /// <summary>
        /// Gets or Sets the read only collection comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string ReadOnlyCollectionCommentTemplate { get; }


        /// <summary>
        /// Builds a comment
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="options">The options </param>
        /// <returns> The comment </returns>
        internal virtual string BuildComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            if (returnType is IdentifierNameSyntax identifier)
            {
                var parent = GetMethodDeclarationSyntax(returnType);
                if (parent != null && parent.TypeParameterList?.Parameters.Any(a => a.Identifier.ValueText == identifier.Identifier.ValueText) == true)
                {
                    var typeParamNode = DocumentationHeaderHelper.CreateTypeParameterRefElementSyntax(identifier.Identifier.ValueText);
                    return typeParamNode.ToFullString();
                }
                return GenerateGeneralComment(identifier.Identifier.ValueText.AsSpan(), true);
            }
            if (returnType is QualifiedNameSyntax qst)
            {
                return GenerateGeneralComment(qst.ToString().AsSpan(), true);
            }
            if (returnType is GenericNameSyntax gst)
            {
                return GenerateGenericTypeComment(gst, options);
            }
            if (returnType is ArrayTypeSyntax ast)
            {
                return string.Format(ArrayCommentTemplate, DetermineSpecificObjectName(ast.ElementType, true));
            }
            if (returnType is PredefinedTypeSyntax pst)
            {
                return GenerateGeneralComment(pst.Keyword.ValueText.AsSpan());
            }
            return GenerateGeneralComment(returnType.ToFullString().AsSpan(), true);
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
        private void BuildChildrenGenericArgList(TypeSyntax argType, List<string> items)
        {
            //bool shouldPluralize;
            if (argType is GenericNameSyntax genericArgType)
            {
                var childArg = genericArgType.TypeArgumentList?.Arguments.FirstOrDefault();
                if (childArg != null)
                {
                    //we check the parent to see if the child needs to be pluralized
                    //shouldPluralize = ShouldPluralize(argType, pluaralizeName);
                    BuildChildrenGenericArgList(childArg, items);
                }
            }
            items.Add(DetermineSpecificObjectName(argType, false, true));
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
                result = pluaralizeIdentifierType ? Pluralizer.Pluralize(value) : value;
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
        /// Generates general comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private string GenerateGeneralComment(ReadOnlySpan<char> returnType, bool returnCref = false)
        {
            var rt = returnType.ToString();
            if (returnCref)
            {
                return $"<see cref=\"{rt}\"/>";
            }
            return rt;
        }

        /// <summary>
        /// Generates generic type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The string </returns>
        private string GenerateGenericTypeComment(GenericNameSyntax returnType, ReturnTypeBuilderOptions options)
        {
            // this will return the full generic Ex. Task<Request>- which then will get added to a CDATA
            if (options.ReturnGenericTypeAsFullString)
            {
                return returnType.ToString();
            }

            var genericTypeStr = returnType.Identifier.ValueText;
            if (returnType.IsReadOnlyCollection())
            {
                var argType = returnType.TypeArgumentList.Arguments.First();

                var items = new List<string>();
                BuildChildrenGenericArgList(argType, items);
                var returnName = DetermineSpecificObjectName(returnType, false, true).ToLower();
                items.Add(returnName);
                items.Reverse();

                var comment = items.ToLowerParts(true)
                                    .PluaralizeLastWord()
                                    .Tap((parts) => {
                                        for (var i = 0; i < parts.Count; i++)
                                        {
                                            parts[i] = parts[i].Replace(returnName, $"a {returnName}");
                                        }
                                    })
                                    .JoinToString(" of ")
                                    .ApplyUserTranslations()
                                    .WithPeriod();
                if (options.IsRootReturnType)
                {
                    comment = comment.ToTitleCase();
                }
                return comment;
            }

            // IEnumerable IList List
            if (returnType.IsList())
            {
                var argType = returnType.TypeArgumentList.Arguments.First();
                var items = new List<string>();
                BuildChildrenGenericArgList(argType, items);

                var returnName = DetermineSpecificObjectName(returnType, false, true).ToLower();
                items.Add(returnName);
                items.Reverse();

                var comment = items.ToLowerParts(true)
                                    .PluaralizeLastWord()
                                    .Tap((parts) =>
                                    {
                                        for (var i = 0; i < parts.Count; i++)
                                        {
                                            parts[i] = parts[i].Replace(returnName, $"a {returnName}");
                                        }
                                    })
                                    .JoinToString(" of ")
                                    .ApplyUserTranslations()
                                    .WithPeriod();
                //var resultStr = string.Join(" of ", items).ToLowerInvariant();
                //var comment = string.Format(ListCommentTemplate, resultStr);
                if (options.IsRootReturnType)
                {
                    comment = comment.ToTitleCase();
                }
                return comment;
            }

            if (returnType.IsDictionary())
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    var argType1 = returnType.TypeArgumentList.Arguments.First();
                    var argType2 = returnType.TypeArgumentList.Arguments.Last();
                    var items = new List<string>();
                    BuildChildrenGenericArgList(argType2, items); //pluaralizeIdentifierType: false
                    var returnName = DetermineSpecificObjectName(returnType, false, true).ToLower();
                    items.Add(returnName);
                    items.Reverse();

                    var comment = items.ToLowerParts(true)
                                   .PluaralizeLastWord()
                                   .Tap((parts) =>
                                   {
                                       for (var i = 0; i < parts.Count; i++)
                                       {
                                           parts[i] = parts[i].Replace(returnName, $"a {returnName}");
                                       }
                                   })
                                   .JoinToString(" of ")
                                   .ApplyUserTranslations()
                                   .WithPeriod();


                    //var resultStr = string.Join(" of ", items).ToLowerInvariant();
                    //var comment = string.Format(DictionaryCommentTemplate, argType1.ApplyUserTranslations(), resultStr);
                    if (options.IsRootReturnType)
                    {
                        //This ensure the return string has correct casing
                        comment = comment.ToTitleCase();
                    }
                    return comment;
                }
                return GenerateGeneralComment(genericTypeStr.AsSpan());
            }

            if (returnType.IsTask() || returnType.IsGenericActionResult() || returnType.IsGenericValueTask())
            {
                var prefix = "and return a <see cref=\"Task\"/> of type ";
                if (returnType.IsGenericActionResult())
                {
                    prefix = "and return an <see cref=\"ActionResult\"/> of type ";
                }
                if (returnType.IsGenericValueTask())
                {
                    prefix = "and return a <see cref=\"ValueTask\"/> of type ";
                }
                if (returnType.TypeArgumentList.Arguments.Count == 1)
                {
                    var firstType = returnType.TypeArgumentList.Arguments.First();
                    if (firstType.IsReadOnlyCollection() || firstType.IsDictionary() || firstType.IsList())
                    {
                        prefix = prefix.Replace("type ", "");
                    }

                    //List<string> items = new List<string>();
                    //BuildChildrenGenericArgList(firstType, items, false, false);
                    //TODO: fix comments flow to support cref nodes
                    var newOptions = new ReturnTypeBuilderOptions
                    {
                        IsRootReturnType = false,
                        ReturnGenericTypeAsFullString = options.ReturnGenericTypeAsFullString,
                        UseProperCasing = options.UseProperCasing,
                        ForcePredefinedTypeEvaluation = true //maybe??
                    };
                    var buildComment = BuildComment(firstType, newOptions);
                    var comment = prefix + buildComment;

                    return comment;
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
                    var newOptions = new ReturnTypeBuilderOptions
                    {
                        IsRootReturnType = false,
                        ReturnGenericTypeAsFullString = options.ReturnGenericTypeAsFullString,
                        UseProperCasing = options.UseProperCasing,
                        ForcePredefinedTypeEvaluation = true
                    };
                    builder.Append($"{BuildComment(item, newOptions)}");
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
