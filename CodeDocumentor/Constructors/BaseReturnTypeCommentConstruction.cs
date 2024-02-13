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
                return GenerateGeneralComment(identifier.Identifier.ValueText.AsSpan(), options.TryToIncludeCrefsForReturnTypes, options.IncludeReturnStatementInGeneralComments);
            }
            if (returnType is QualifiedNameSyntax qst)
            {
                return GenerateGeneralComment(qst.ToString().AsSpan(), options.TryToIncludeCrefsForReturnTypes, options.IncludeReturnStatementInGeneralComments);
            }
            if (returnType is GenericNameSyntax gst)
            {
                return GenerateGenericTypeComment(gst, options);
            }
            if (returnType is ArrayTypeSyntax ast)
            {
                var comment = string.Format(ArrayCommentTemplate, DocumentationHeaderHelper.DetermineSpecificObjectName(ast.ElementType, options.TryToIncludeCrefsForReturnTypes));
                return GenerateGeneralComment(comment.AsSpan(), false, options.TryToIncludeCrefsForReturnTypes);
            }
            return returnType is PredefinedTypeSyntax pst
                ? GenerateGeneralComment(pst.Keyword.ValueText.AsSpan(), options.TryToIncludeCrefsForReturnTypes, options.IncludeReturnStatementInGeneralComments)
                : GenerateGeneralComment(returnType.ToFullString().AsSpan(), options.TryToIncludeCrefsForReturnTypes, options.IncludeReturnStatementInGeneralComments);
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
        private string GenerateGeneralComment(ReadOnlySpan<char> returnType, bool returnCref = false, bool includeReturnStatement = false)
        {
            var rt = returnType.ToString();
            if (includeReturnStatement)
            {
                var startWord = DocumentationHeaderHelper.DetermineStartingWord(rt.AsSpan(), false);
                return returnCref ? $"Returns {startWord} <see cref=\"{rt}\"/>" : $"Returns {startWord} {rt}";
            }
            return returnCref ? $"<see cref=\"{rt}\"/>" : rt;
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
                return GenerateGeneralComment(genericTypeStr.AsSpan());
            }

            if (returnType.IsTask() || returnType.IsGenericActionResult() || returnType.IsGenericValueTask())
            {
                return returnType.TypeArgumentList.Arguments.Count == 1
                    ? GenericCommentManager.ProcessSingleTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts))
                    : GenericCommentManager.ProcessMultiTypeTaskArguments(returnType, options, (typeSyntax, opts) => BuildComment(typeSyntax, opts));
            }
            return GenerateGeneralComment(genericTypeStr.AsSpan());
        }
    }
}
