﻿using System;
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
    public class MethodCodeFixProvider : BaseCodeFixProvider
    {
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
            var displayTitle = declaration.HasSummary() ? titleRebuild : title;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: displayTitle,
                    createChangedDocument: c => AddDocumentationHeaderAsync(context.Document, root, declaration, c),
                    equivalenceKey: displayTitle),
                diagnostic);

            await RegisterFileCodeFixesAsync(context, diagnostic);
        }

        /// <summary>
        ///   Gets the exceptions from the body
        /// </summary>
        /// <param name="textToSearch"> </param>
        /// <returns> </returns>
    
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
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            var summaryText = CommentHelper.CreateMethodComment(declarationSyntax.Identifier.ValueText, declarationSyntax.ReturnType);

            list = list.WithSummary(declarationSyntax,summaryText, optionsService.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithParameters(declarationSyntax)
                        .WithExceptionTypes(declarationSyntax)
                        .WithExisting(declarationSyntax, DocumentationHeaderHelper.REMARKS)
                        .WithExisting(declarationSyntax, DocumentationHeaderHelper.EXAMPLE)
                        .WithReturnType(declarationSyntax);

            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

    }
}
