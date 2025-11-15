using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Classes;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor
{
    /// <summary>
    ///  The class code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassCodeFixProvider)), Shared]
    public class ClassCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this class";

        private const string TitleRebuild = "Code Documentor update this class";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.CreateRange(new List<string> {
            ClassAnalyzerSettings.DiagnosticId
        });

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
            var declaration = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }

            var settings = await context.BuildSettingsAsync();

            if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declaration))
            {
                return;
            }
            var displayTitle = declaration.HasSummary() ? TitleRebuild : Title;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: displayTitle,
                    createChangedDocument: c => AddDocumentationHeaderAsync(settings, context.Document, root, declaration, c),
                    equivalenceKey: displayTitle),
                diagnostic);

            await RegisterFileCodeFixesAsync(context, diagnostic);
        }

        /// <summary>
        ///  Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        internal Task<Document> AddDocumentationHeaderAsync(ISettings settings, Document document, SyntaxNode root, ClassDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
                {
                    var newDeclaration = ServiceLocator.CommentBuilderService.BuildNewDeclaration(declarationSyntax);
                    var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                    return document.WithSyntaxRoot(newRoot);
                }, ClassAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
