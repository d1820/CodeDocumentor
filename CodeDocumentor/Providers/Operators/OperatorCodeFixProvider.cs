using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Operators;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OperatorCodeFixProvider)), Shared]
    public class OperatorCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Code Documentor this operator";
        private const string TitleRebuild = "Code Documentor update this operator";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OperatorAnalyzerSettings.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root?.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<OperatorDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }
            var settings = await context.BuildSettingsAsync();
            var displayTitle = declaration.HasSummary() ? TitleRebuild : Title;
            context.RegisterCodeFix(CodeAction.Create(title: displayTitle,
                createChangedDocument: c => AddDocumentationHeaderAsync(settings, context.Document, root, declaration, c),
                equivalenceKey: displayTitle), diagnostic);
            await RegisterFileCodeFixesAsync(context, diagnostic);
        }

        private Task<Document> AddDocumentationHeaderAsync(ISettings settings, Document document, SyntaxNode root, OperatorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            return Task.Run(() => TryHelper.Try(() =>
            {
                var newDeclaration = ServiceLocator.CommentBuilderService.BuildNewDeclaration(settings, declarationSyntax);
                var newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
                return document.WithSyntaxRoot(newRoot);
            }, OperatorAnalyzerSettings.DiagnosticId, EventLogger, (_) => document, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.ADD_DOCUMENTATION_HEADER), cancellationToken);
        }
    }
}
