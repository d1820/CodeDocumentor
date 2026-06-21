using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Events;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxNode = Microsoft.CodeAnalysis.SyntaxNode;

namespace CodeDocumentor
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EventCodeFixProvider)), Shared]
    public class EventCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this event";
        private const string TitleRebuild = "Code Documentor update this event";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EventAnalyzerSettings.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var settings = await context.BuildSettingsAsync();

            var eventFieldDecl = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<EventFieldDeclarationSyntax>().FirstOrDefault();
            if (eventFieldDecl != null)
            {
                if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(eventFieldDecl))
                {
                    return;
                }
                var displayTitle = eventFieldDecl.HasSummary() ? TitleRebuild : Title;
                context.RegisterCodeFix(CodeAction.Create(title: displayTitle,
                    createChangedDocument: c => AddEventFieldDocAsync(settings, context.Document, root, eventFieldDecl, c),
                    equivalenceKey: displayTitle), diagnostic);
                await RegisterFileCodeFixesAsync(context, diagnostic);
                return;
            }

            var eventDecl = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<EventDeclarationSyntax>().FirstOrDefault();
            if (eventDecl != null)
            {
                if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(eventDecl))
                {
                    return;
                }
                var displayTitle = eventDecl.HasSummary() ? TitleRebuild : Title;
                context.RegisterCodeFix(CodeAction.Create(title: displayTitle,
                    createChangedDocument: c => AddEventDocAsync(settings, context.Document, root, eventDecl, c),
                    equivalenceKey: displayTitle), diagnostic);
                await RegisterFileCodeFixesAsync(context, diagnostic);
            }
        }

        private Task<Document> AddEventFieldDocAsync(ISettings settings, Document document, SyntaxNode root, EventFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = ServiceLocator.CommentBuilderService.BuildNewDeclaration(settings, declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, EventAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }

        private Task<Document> AddEventDocAsync(ISettings settings, Document document, SyntaxNode root, EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = ServiceLocator.CommentBuilderService.BuildNewDeclaration(settings, declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, EventAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
