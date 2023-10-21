﻿using System;
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
    /// <summary>
    ///   The property code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PropertyCodeFixProvider)), Shared]
    public class PropertyCodeFixProvider : CodeFixProvider
    {
        private const string title = "Code Documentor this property";
        private const string titleRebuild = "Code Documentor update this property";

        /// <summary>
        ///   Gets the fixable diagnostic ids.
        /// </summary>
        public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PropertyAnalyzerSettings.DiagnosticId);

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

            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declaration))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: declaration.HasSummary() ? titleRebuild : title,
                    createChangedDocument: c => AddDocumentationHeaderAsync(context.Document, root, declaration, c),
                    equivalenceKey: declaration.HasSummary() ? titleRebuild : title),
                diagnostic);
        }

        /// <summary>
        /// Builds the comments. This is only used in the file level fixProvider.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="nodesToReplace">The nodes to replace.</param>
        internal static int BuildComments(SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.PropertyDeclaration)).OfType<PropertyDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
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
            return neededCommentCount;
        }

        private static PropertyDeclarationSyntax BuildNewDeclaration(PropertyDeclarationSyntax declarationSyntax)
        {
            SyntaxList<XmlNodeSyntax> list = SyntaxFactory.List<XmlNodeSyntax>();

            bool isBoolean = false;
            if (declarationSyntax.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = ((PredefinedTypeSyntax)declarationSyntax.Type).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (declarationSyntax.Type.IsKind(SyntaxKind.NullableType))
            {
                var returnType = ((NullableTypeSyntax)declarationSyntax.Type).ElementType as PredefinedTypeSyntax;
                if (returnType != null)
                {
                    isBoolean = returnType.ToString().IndexOf("bool", StringComparison.OrdinalIgnoreCase) > -1;
                }
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

            string propertyComment = CommentHelper.CreatePropertyComment(declarationSyntax.Identifier.ValueText, isBoolean, hasSetter);
            list = list.AddRange(DocumentationHeaderHelper.CreateSummaryPartNodes(propertyComment));
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.IncludeValueNodeInProperties)
            {
                string returnComment = new ReturnCommentConstruction().BuildComment(declarationSyntax.Type, false);
                list = list.AddRange(DocumentationHeaderHelper.CreateValuePartNodes(returnComment));
            }
            list = list.AttachExistingNodeSyntax(declarationSyntax, "remarks").AttachExistingNodeSyntax(declarationSyntax, "example");

            DocumentationCommentTriviaSyntax commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            SyntaxTriviaList leadingTrivia = declarationSyntax.GetLeadingTrivia();
            PropertyDeclarationSyntax newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
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
            var newDeclaration = BuildNewDeclaration(declarationSyntax);
            SyntaxNode newRoot = root.ReplaceNode(declarationSyntax, newDeclaration);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
