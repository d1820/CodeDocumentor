using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
    /// <summary> The record code fix provider. </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RecordCodeFixProvider)), Shared]
    public class RecordCodeFixProvider : BaseCodeFixProvider
    {
        /// <summary> Gets the fixable diagnostic ids. </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RecordAnalyzerSettings.DiagnosticId);

        /// <summary> Gets fix all provider. </summary>
        /// <returns> A FixAllProvider. </returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary> Registers code fixes async. </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<RecordDeclarationSyntax>().First();
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declaration))
            {
                return;
            }
            var displayTitle = declaration.HasSummary() ? TitleRebuild : Title;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: displayTitle,
                    createChangedDocument: c => AddDocumentationHeaderAsync(context.Document, root, declaration, c),
                    equivalenceKey: displayTitle),
                diagnostic);

            await RegisterFileCodeFixesAsync(context, diagnostic);
        }

        /// <summary> Adds documentation header async. </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        internal static async Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, RecordDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, cancellationToken);
        }

        /// <summary> Builds the comments. This is only used in the file level fixProvider. </summary>
        /// <param name="root"> The root. </param>
        /// <param name="nodesToReplace"> The nodes to replace. </param>
        internal static int BuildComments(SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.RecordDeclaration)).OfType<RecordDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            foreach (var declarationSyntax in declarations)
            {
                if (optionsService.IsEnabledForPublicMembersOnly
                    && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                {
                    continue;
                }
                if (declarationSyntax.HasSummary()) //if record already has comments dont redo it. User should update this manually
                {
                    continue;
                }
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                neededCommentCount++;
            }
            return neededCommentCount;
        }

        private const string Title = "Code Documentor this record";

        private const string TitleRebuild = "Code Documentor update this record";

        private static RecordDeclarationSyntax BuildNewDeclaration(RecordDeclarationSyntax declarationSyntax)
        {
            var list = SyntaxFactory.List<XmlNodeSyntax>();
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            var comment = CommentHelper.CreateRecordComment(declarationSyntax.Identifier.ValueText);
            list = list.WithSummary(declarationSyntax, comment, optionsService.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithExisting(declarationSyntax, DocumentationHeaderHelper.REMARKS)
                        .WithExisting(declarationSyntax, DocumentationHeaderHelper.EXAMPLE);

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            //append to any existing leading trivia [attributes, decorators, etc)
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();

            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }
    }
}
