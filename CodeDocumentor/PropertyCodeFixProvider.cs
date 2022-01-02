using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeDocumentor.Helper;
using CodeDocumentor.Settings;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor
{
    /// <summary>
    ///   The property code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyCodeFixProvider)), Shared]
    public class PropertyCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        ///   The title.
        /// </summary>
        private const string title = "Add documentation header to this property";

        /// <summary>
        ///   Gets the fixable diagnostic ids.
        /// </summary>
        public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PropertyAnalyzer.DiagnosticId);

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

            PropertyDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().First();

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
        ///   Adds documentation header async.
        /// </summary>
        /// <param name="document"> The document. </param>
        /// <param name="root"> The root. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A Document. </returns>
        private async Task<Document> AddDocumentationHeaderAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken)
        {
            SyntaxList<SyntaxNode> list = SyntaxFactory.List<SyntaxNode>();

            bool isBoolean = false;
            if (declarationSyntax.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = ((PredefinedTypeSyntax)declarationSyntax.Type).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (declarationSyntax.Type.IsKind(SyntaxKind.NullableType))
            {
                var retrunType = ((NullableTypeSyntax)declarationSyntax.Type).ElementType as PredefinedTypeSyntax;
                isBoolean = retrunType.ToString().IndexOf("bool", StringComparison.OrdinalIgnoreCase)  > -1;
            }

            bool hasSetter = false;

            if (declarationSyntax.AccessorList != null && declarationSyntax.AccessorList.Accessors.Any(o => o.Kind() == SyntaxKind.SetAccessorDeclaration))
            {
                if (declarationSyntax.AccessorList.Accessors.First(o => o.Kind() == SyntaxKind.SetAccessorDeclaration).ChildTokens().Any(o => o.Kind() == SyntaxKind.PrivateKeyword || o.Kind() == SyntaxKind.InternalKeyword))
                {
                    // private set or internal set should consider as no set.
                    hasSetter = false;
                }
                else
                {
                    hasSetter = true;
                }
            }

            string propertyComment = CommentHelper.CreatePropertyComment(declarationSyntax.Identifier.ValueText.AsSpan(), isBoolean, hasSetter);
            list = list.AddRange(DocumentationHeaderHelper.CreateSummaryPartNodes(propertyComment));

            if (CodeDocumentorPackage.Options.IncludeValueNodeInProperties)
            {
                string returnComment = new ReturnCommentConstruction().BuildComment(declarationSyntax.Type, false);
                list = list.AddRange(DocumentationHeaderHelper.CreateValuePartNodes(returnComment));
            }

            DocumentationCommentTriviaSyntax commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            SyntaxTriviaList leadingTrivia = declarationSyntax.GetLeadingTrivia();
            SyntaxTriviaList newLeadingTrivia = leadingTrivia.Insert(leadingTrivia.Count - 1, SyntaxFactory.Trivia(commentTrivia));
            PropertyDeclarationSyntax newDeclaration = declarationSyntax.WithLeadingTrivia(newLeadingTrivia);

            SyntaxNode newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
