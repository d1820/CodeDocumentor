using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Builders;
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
    ///  The constructor code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorCodeFixProvider)), Shared]
    public class ConstructorCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this constructor";

        private const string TitleRebuild = "Code Documentor update this constructor";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConstructorAnalyzerSettings.DiagnosticId);

        /// <summary>
        ///  Gets fix all provider.
        /// </summary>
        /// <returns> A FixAllProvider. </returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        ///  Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }
            var optionsService = OptionsService;
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

        /// <summary>
        ///  Builds the comments. This is only used in the file level fixProvider.
        /// </summary>
        /// <param name="root"> The root. </param>
        /// <param name="nodesToReplace"> The nodes to replace. </param>
        /// <returns> An int. </returns>
        internal static int BuildComments(SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ConstructorDeclaration)).OfType<ConstructorDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                var optionsService = OptionsService;
                foreach (var declarationSyntax in declarations)
                {
                    if (optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, ConstructorAnalyzerSettings.DiagnosticId, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        private static ConstructorDeclarationSyntax BuildNewDeclaration(ConstructorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateDocumentationCommentTriviaSyntax(declarationSyntax);
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        /// <summary>
        ///  Creates documentation comment trivia syntax.
        /// </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A DocumentationCommentTriviaSyntax. </returns>
        private static DocumentationCommentTriviaSyntax CreateDocumentationCommentTriviaSyntax(ConstructorDeclarationSyntax declarationSyntax)
        {
            var optionsService = OptionsService;
            var comment = CommentHelper.CreateConstructorComment(declarationSyntax.Identifier.ValueText, declarationSyntax.IsPrivate());
            var builder = CodeDocumentorPackage.DIContainer().GetInstance<DocumentationBuilder>();
            var list = builder.WithSummary(declarationSyntax, comment, optionsService.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, OptionsService)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();

            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

        /// <summary>
        ///  Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        private Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, ConstructorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, ConstructorAnalyzerSettings.DiagnosticId, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
