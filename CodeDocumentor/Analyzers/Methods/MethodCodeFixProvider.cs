using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor
{
    /// <summary>
    ///   The method code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodCodeFixProvider)), Shared]
    public class MethodCodeFixProvider : CodeFixProvider
    {
        private static Regex regEx = new Regex(@"throw\s+new\s+\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private const string title = "Code Documentor this method";

        private const string titleRebuild = "Code Documentor update this method";

        /// <summary>
        ///   Gets the fixable diagnostic ids.
        /// </summary>
        public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodAnalyzerSettings.DiagnosticId);

        /// <summary>
        ///   Gets fix all provider.
        /// </summary>
        /// <returns> A FixAllProvider. </returns>
        public override sealed FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        ///   Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            MethodDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declaration))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: declaration.HasSummary() ? titleRebuild : title,
                    createChangedDocument: c => AddDocumentationHeaderAsync(context.Document, root, declaration, c),
                    equivalenceKey: declaration.HasSummary() ? titleRebuild : title),
                diagnostic);
        }

        /// <summary>
        ///   Gets the exceptions from the body
        /// </summary>
        /// <param name="textToSearch"> </param>
        /// <returns> </returns>
        internal static IEnumerable<string> GetExceptions(string textToSearch)
        {
            if (string.IsNullOrEmpty(textToSearch))
            {
                return Enumerable.Empty<string>();
            }
            var exceptions = regEx.Matches(textToSearch).OfType<Match>()
                                                        .Select(m => m?.Groups[0]?.Value)
                                                        .Distinct();
            return exceptions;
        }

        /// <summary>
        ///   Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Task. </returns>
        private async Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, MethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return await Task.Run(() => {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                SyntaxNode newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, cancellationToken);
           
        }

        private static MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax)
        {
            SyntaxTriviaList leadingTrivia = declarationSyntax.GetLeadingTrivia();
            DocumentationCommentTriviaSyntax commentTrivia = CreateDocumentationCommentTriviaSyntax(declarationSyntax);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        /// <summary>
        /// Builds the comments. This is only used in the file level fixProvider.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="nodesToReplace">The nodes to replace.</param>
        internal static int BuildComments(SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            foreach (var declarationSyntax in declarations)
            {
                if (optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                {
                    continue;
                }
                //if method is already commented dont redo it, user should update methods indivually
                if (declarationSyntax.HasSummary())
                {
                    continue;
                }
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                neededCommentCount++;
            }
            return neededCommentCount;
        }

        /// <summary>
        ///   Creates documentation comment trivia syntax.
        /// </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A DocumentationCommentTriviaSyntax. </returns>
        private static DocumentationCommentTriviaSyntax CreateDocumentationCommentTriviaSyntax(MethodDeclarationSyntax declarationSyntax)
        {
            SyntaxList<XmlNodeSyntax> list = SyntaxFactory.List<XmlNodeSyntax>();
            var commentXmlSyntax = GetSummaryXmlSyntax(declarationSyntax);
            list = list.AddRange(commentXmlSyntax);

            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (TypeParameterSyntax parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateTypeParameterPartNodes(parameter.Identifier.ValueText));
                }
            }

            if (declarationSyntax?.ParameterList?.Parameters.Any() == true)
            {
                foreach (ParameterSyntax parameter in declarationSyntax.ParameterList.Parameters)
                {
                    string parameterComment = CommentHelper.CreateParameterComment(parameter);
                    list = list.AddRange(DocumentationHeaderHelper.CreateParameterPartNodes(parameter.Identifier.ValueText, parameterComment));
                }
            }

            var exceptions = GetExceptions(declarationSyntax.Body?.ToFullString());

            if (exceptions.Any())
            {
                foreach (var exception in exceptions)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateExceptionNodes(exception));
                }
            }

            list = list.AttachExistingNodeSyntax(declarationSyntax, "remarks").AttachExistingNodeSyntax(declarationSyntax, "example");

            string returnType = declarationSyntax.ReturnType.ToString();
            if (returnType != "void")
            {
                var commentConstructor = new ReturnCommentConstruction(declarationSyntax.ReturnType);
                string returnComment = commentConstructor.Comment;
                list = list.AddRange(DocumentationHeaderHelper.CreateReturnPartNodes(returnComment));
            }

            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

        private static XmlNodeSyntax[] GetSummaryXmlSyntax(MethodDeclarationSyntax declarationSyntax)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.PreserveExistingSummaryText)
            {
                var summary = declarationSyntax.GetElementSyntax("summary");

                if (summary != null)
                {
                    return DocumentationHeaderHelper.WrapElementSyntaxInCommentSyntax(summary);
                }
            }
            var summaryText = CommentHelper.CreateMethodComment(declarationSyntax.Identifier.ValueText, declarationSyntax.ReturnType);
            return DocumentationHeaderHelper.CreateSummaryPartNodes(summaryText); ;
        }
    }
}
