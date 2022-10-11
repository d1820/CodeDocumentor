using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Package;

namespace CodeDocumentor
{
    /// <summary>
    ///   The class code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassCodeFixProvider)), Shared]
    public class ClassCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        ///   The title.
        /// </summary>
        private const string title = "Code Documentor this class";

        /// <summary>
        ///   Gets the fixable diagnostic ids.
        /// </summary>
        public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ClassAnalyzerSettings.DiagnosticId);

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

            ClassDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            if (CodeDocumentorPackage.Options?.IsEnabledForPublishMembersOnly == true && PrivateMemberVerifier.IsPrivateMember(declaration))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => AddDocumentationHeaderAsync(context.Document, root, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        /// <summary>
        /// Adds the documentation headers asynchronously.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="root">The root.</param>
        /// <param name="declarationSyntaxes">The declaration syntaxes.</param>
        /// <returns><![CDATA[Task<Document>]]></returns>
        internal static async Task<Document> AddDocumentationHeadersAsync(Document document, SyntaxNode root, IEnumerable<ClassDeclarationSyntax> declarationSyntaxes)
        {
            var nodesToReplace = new Dictionary<ClassDeclarationSyntax, ClassDeclarationSyntax>();
            foreach (var declarationSyntax in declarationSyntaxes)
            {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                nodesToReplace.Add(declarationSyntax, newDeclaration);
            }

            root = root.ReplaceNodes(nodesToReplace.Keys, (n1, n2) =>
            {
                return nodesToReplace[n1];
            });
            return document.WithSyntaxRoot(root);
        }

        /// <summary>
        ///   Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        internal static async Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, ClassDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            var newDeclaration = BuildNewDeclaration(declarationSyntax);
            SyntaxNode newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ClassDeclarationSyntax BuildNewDeclaration(ClassDeclarationSyntax declarationSyntax)
        {
            SyntaxList<SyntaxNode> list = SyntaxFactory.List<SyntaxNode>();

            string comment = CommentHelper.CreateClassComment(declarationSyntax.Identifier.ValueText.AsSpan());
            list = list.AddRange(DocumentationHeaderHelper.CreateSummaryPartNodes(comment));

            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (TypeParameterSyntax parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateTypeParameterPartNodes(parameter.Identifier.ValueText));
                }
            }

            DocumentationCommentTriviaSyntax commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            //append to any existing leading trivia [attributes, decorators, etc)
            SyntaxTriviaList leadingTrivia = declarationSyntax.GetLeadingTrivia();

            var newLeadingTrivia = DocumentationHeaderHelper.BuildLeadingTrivia(leadingTrivia, commentTrivia);
            ClassDeclarationSyntax newDeclaration = declarationSyntax.WithLeadingTrivia(newLeadingTrivia);
            return newDeclaration;
        }
    }
}
