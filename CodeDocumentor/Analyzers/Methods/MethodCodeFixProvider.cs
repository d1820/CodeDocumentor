using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Builders;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor
{
    /// <summary>
    ///  The method code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodCodeFixProvider)), Shared]
    public class MethodCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this method";

        private const string TitleRebuild = "Code Documentor update this method";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodAnalyzerSettings.DiagnosticId);

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

            var declaration = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }
            var optionsService = OptionsService;
            if (
                //NOTE: Since interfaces declarations do not have accessors, we allow documenting all the time.
                !declaration.IsOwnedByInterface() &&
                optionsService.IsEnabledForPublicMembersOnly &&
                PrivateMemberVerifier.IsPrivateMember(declaration)
                )
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
        internal static int BuildComments(SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                var optionsService = OptionsService;
                foreach (var declarationSyntax in declarations)
                {
                    if (
                       !declarationSyntax.IsOwnedByInterface() &&
                       optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax)
                    )
                    {
                        continue;
                    }
                    //if method is already commented dont redo it, user should update methods individually
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, MethodAnalyzerSettings.DiagnosticId, EventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        private static MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateDocumentationCommentTriviaSyntax(declarationSyntax);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        /// <summary>
        ///  Creates documentation comment trivia syntax.
        /// </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A DocumentationCommentTriviaSyntax. </returns>
        private static DocumentationCommentTriviaSyntax CreateDocumentationCommentTriviaSyntax(MethodDeclarationSyntax declarationSyntax)
        {
            var optionsService = OptionsService;
            var commentHelper = new CommentHelper();
            var summaryText = commentHelper.CreateMethodComment(declarationSyntax.Identifier.ValueText,
                                                                declarationSyntax.ReturnType,
                                                               optionsService.UseToDoCommentsOnSummaryError,
                                                               optionsService.TryToIncludeCrefsForReturnTypes,
                                                               optionsService.ExcludeAsyncSuffix,
                                                               optionsService.WordMaps);
            var builder = new DocumentationBuilder();

            var list = builder.WithSummary(declarationSyntax, summaryText, optionsService.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithParameters(declarationSyntax, optionsService.WordMaps)
                        .WithExceptionTypes(declarationSyntax)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .WithReturnType(declarationSyntax, optionsService.UseNaturalLanguageForReturnNode, optionsService.TryToIncludeCrefsForReturnTypes, optionsService.WordMaps)
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
        /// <returns> A Task. </returns>
        private Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, MethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, MethodAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
