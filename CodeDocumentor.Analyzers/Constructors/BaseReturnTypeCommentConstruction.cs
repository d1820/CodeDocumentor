using System;
using System.Linq;
using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Locators;
using CodeDocumentor.Analyzers.Managers;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Analyzers.Constructors
{
    public abstract class BaseReturnTypeCommentConstruction
    {
        /// <summary>
        ///  Gets or Sets the array comment template.
        /// </summary>
        /// <value> A string. </value>
        public string ArrayCommentTemplate { get; } = "array of {0}";

        /// <summary>
        ///  Generates the comment.
        /// </summary>
        public string Comment { get; protected set; }

        /// <summary>
        ///  Gets or Sets the dictionary comment template.
        /// </summary>
        /// <value> A string. </value>
        public abstract string DictionaryCommentTemplate { get; }

        protected GenericCommentManager GenericCommentManager { get; } = ServiceLocator.GenericCommentManager;
        protected DocumentationHeaderHelper DocumentationHeaderHelper { get; } = ServiceLocator.DocumentationHeaderHelper;


        /// <summary>
        ///  Builds a comment
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="options"> The options </param>
        /// <returns> The comment </returns>
        public virtual string BuildComment(TypeSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            var returnComment = string.Empty;
            if (returnType is IdentifierNameSyntax identifier)
            {
                var parent = GetMethodDeclarationSyntax(returnType);
                if (parent != null && parent.TypeParameterList?.Parameters.Any(a => a.Identifier.ValueText == identifier.Identifier.ValueText) == true)
                {
                    var typeParamNode = DocumentationHeaderHelper.CreateElementWithAttributeSyntax("typeparamref", "name", identifier.Identifier.ValueText);
                    if (options.IncludeStartingWordInText)
                    {
                        var startWord = DocumentationHeaderHelper.DetermineStartingWord(identifier.Identifier.ValueText.AsSpan(), options.UseProperCasing);
                        if (!string.IsNullOrEmpty(startWord))
                        {
                            returnComment = $"{startWord} {typeParamNode.ToFullString()}";
                        }
                        else
                        {
                            returnComment = typeParamNode.ToFullString();
                        }
                    }
                    else
                    {
                        returnComment = typeParamNode.ToFullString();
                    }
                }
                else
                {
                    returnComment = GenerateGeneralComment(identifier.Identifier.ValueText.AsSpan(), options);
                }
            }
            else if (returnType is QualifiedNameSyntax qst)
            {
                returnComment = GenerateGeneralComment(qst.ToString().AsSpan(), options);
            }
            else if (returnType is GenericNameSyntax gst)
            {
                returnComment = GenerateGenericTypeComment(gst, options, wordMaps);
            }
            else if (returnType is ArrayTypeSyntax ast)
            {
                var comment = string.Format(ArrayCommentTemplate, DocumentationHeaderHelper.DetermineSpecificObjectName(ast.ElementType, wordMaps, options.TryToIncludeCrefsForReturnTypes));
                var arrayOptions = options.Clone();
                arrayOptions.IncludeStartingWordInText = true;
                arrayOptions.TryToIncludeCrefsForReturnTypes = false;
                returnComment = GenerateGeneralComment(comment.AsSpan(), arrayOptions);
            }
            else
            {
                returnComment = returnType is PredefinedTypeSyntax pst
                ? GenerateGeneralComment(pst.Keyword.ValueText.AsSpan(), options)
                : GenerateGeneralComment(returnType.ToFullString().AsSpan(), options);
            }
            return returnComment;
        }

        /// <summary>
        ///  Finds the parent MethodDeclarationSyntax if exists
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> </returns>
        private static MethodDeclarationSyntax GetMethodDeclarationSyntax(SyntaxNode node)
        {
            return !(node is MethodDeclarationSyntax) && node.Parent != null
                ? GetMethodDeclarationSyntax(node.Parent)
                : node as MethodDeclarationSyntax;
        }

        /// <summary>
        ///  Generates general comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The comment. </returns>
        private string GenerateGeneralComment(ReadOnlySpan<char> returnType, ReturnTypeBuilderOptions options)
        {
            var rt = returnType.ToString().Trim();
            string startWord = "";
            if (options.IncludeStartingWordInText)
            {
                startWord = DocumentationHeaderHelper.DetermineStartingWord(rt.AsSpan(), options.UseProperCasing);
            }
            return (options.TryToIncludeCrefsForReturnTypes ? $"{startWord} <see cref=\"{rt}\"/>" : $"{startWord} {rt}").Trim();
        }

        /// <summary>
        ///  Generates generic type comment.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <returns> The string </returns>
        private string GenerateGenericTypeComment(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            // this will return the full generic Ex. Task<Request>- which then will get added to a CDATA
            if (options.ReturnGenericTypeAsFullString)
            {
                return returnType.ToString();
            }

            var genericTypeStr = returnType.Identifier.ValueText;
            if (returnType.IsReadOnlyCollection())
            {
                var comment = GenericCommentManager.ProcessReadOnlyCollection(returnType, options, wordMaps);
                return comment;
            }

            // IEnumerable IList List
            if (returnType.IsList())
            {
                var comment = GenericCommentManager.ProcessList(returnType, options, wordMaps);
                return comment;
            }

            if (returnType.IsDictionary())
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    var comment = GenericCommentManager.ProcessDictionary(returnType, options, DictionaryCommentTemplate, wordMaps);
                    return comment;
                }
                return GenerateGeneralComment(genericTypeStr.AsSpan(), new ReturnTypeBuilderOptions());
            }

            if (returnType.IsTask() || returnType.IsGenericActionResult() || returnType.IsGenericValueTask())
            {
                return returnType.TypeArgumentList.Arguments.Count == 1
                    ? GenericCommentManager.ProcessSingleTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts, wordMaps))
                    : GenericCommentManager.ProcessMultiTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts, wordMaps));
            }
            return GenerateGeneralComment(genericTypeStr.AsSpan(), new ReturnTypeBuilderOptions());
        }
    }
}
