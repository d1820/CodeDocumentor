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
    ///  The property code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyCodeFixProvider)), Shared]
    public class PropertyCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this property";

        private const string TitleRebuild = "Code Documentor update this property";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PropertyAnalyzerSettings.DiagnosticId);

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

            var declaration = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }
            var settings = Settings;
            if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declaration))
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
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.PropertyDeclaration)).OfType<PropertyDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                var settings = Settings;
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
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
            }, PropertyAnalyzerSettings.DiagnosticId, EventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        private static PropertyDeclarationSyntax BuildNewDeclaration(PropertyDeclarationSyntax declarationSyntax)
        {
            var isBoolean = declarationSyntax.IsPropertyReturnTypeBool();

            var hasSetter = declarationSyntax.PropertyHasSetter();
            var settings = Settings;
            var commentHelper = new CommentHelper();
            var propertyComment = commentHelper.CreatePropertyComment(declarationSyntax.Identifier.ValueText, isBoolean,
                                                                        hasSetter, settings.ExcludeAsyncSuffix, settings.WordMaps);
            var builder = new DocumentationBuilder();

            var returnOptions = new ReturnTypeBuilderOptions
            {
                TryToIncludeCrefsForReturnTypes = settings.TryToIncludeCrefsForReturnTypes,
                GenerateReturnStatement = settings.IncludeValueNodeInProperties,
                ReturnGenericTypeAsFullString = false,
                IncludeStartingWordInText = true,
                UseProperCasing = true
            };
            var list = builder.WithSummary(declarationSyntax, propertyComment, settings.PreserveExistingSummaryText)
                        .WithPropertyValueTypes(declarationSyntax, returnOptions, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        /// <summary>
        ///  Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        private Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = BuildNewDeclaration(declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, PropertyAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
