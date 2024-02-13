using System;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Managers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Constructors
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

        public GenericCommentManager GenericCommentManager => CodeDocumentorPackage.DIContainer().GetInstance<GenericCommentManager>();

        /// <summary>
        ///  Builds a comment
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="options"> The options </param>
        /// <returns> The comment </returns>
        internal virtual string BuildComment(TypeSyntax returnType, ReturnTypeBuilderOptions options)
        {
            if (returnType is IdentifierNameSyntax identifier)
            {
                var parent = GetMethodDeclarationSyntax(returnType);
                if (parent != null && parent.TypeParameterList?.Parameters.Any(a => a.Identifier.ValueText == identifier.Identifier.ValueText) == true)
                {
                    var typeParamNode = DocumentationHeaderHelper.CreateElementWithAttributeSyntax("typeparamref", "name", identifier.Identifier.ValueText);
                    return typeParamNode.ToFullString();
                }
                return GenerateGeneralComment(identifier.Identifier.ValueText.AsSpan(), options);
            }
            if (returnType is QualifiedNameSyntax qst)
            {
                return GenerateGeneralComment(qst.ToString().AsSpan(), options);
            }
            if (returnType is GenericNameSyntax gst)
            {
                return GenerateGenericTypeComment(gst, options);
            }
            if (returnType is ArrayTypeSyntax ast)
            {
                var comment = string.Format(ArrayCommentTemplate, DocumentationHeaderHelper.DetermineSpecificObjectName(ast.ElementType, options.TryToIncludeCrefsForReturnTypes));
                var arrayOptions = options.Clone();
                arrayOptions.IncludeStartingWordInText = true;
                arrayOptions.TryToIncludeCrefsForReturnTypes = false;
                return GenerateGeneralComment(comment.AsSpan(), arrayOptions);
            }
            return returnType is PredefinedTypeSyntax pst
                ? GenerateGeneralComment(pst.Keyword.ValueText.AsSpan(), options)
                : GenerateGeneralComment(returnType.ToFullString().AsSpan(), options);
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
            var rt = returnType.ToString();
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
                var comment = GenericCommentManager.ProcessReadOnlyCollection(returnType, options);
                return comment;
            }

            // IEnumerable IList List
            if (returnType.IsList())
            {
                var comment = GenericCommentManager.ProcessList(returnType, options);
                return comment;
            }

            if (returnType.IsDictionary())
            {
                if (returnType.TypeArgumentList.Arguments.Count == 2)
                {
                    var comment = GenericCommentManager.ProcessDictionary(returnType, options, DictionaryCommentTemplate);
                    return comment;
                }
                return GenerateGeneralComment(genericTypeStr.AsSpan(), new ReturnTypeBuilderOptions());
            }

            if (returnType.IsTask() || returnType.IsGenericActionResult() || returnType.IsGenericValueTask())
            {
                return returnType.TypeArgumentList.Arguments.Count == 1
                    ? GenericCommentManager.ProcessSingleTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts))
                    : GenericCommentManager.ProcessMultiTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts));
            }
            return GenerateGeneralComment(genericTypeStr.AsSpan(), new ReturnTypeBuilderOptions());
        }
    }
}
