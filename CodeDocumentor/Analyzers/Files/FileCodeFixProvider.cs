using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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

namespace CodeDocumentor
{

    /// <summary>
    ///   The class code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileCodeFixProvider)), Shared]
    public class FileCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        ///   The title.
        /// </summary>
        private const string title = "Code Documentor this whole file";

        /// <summary>
        ///   Gets the fixable diagnostic ids.
        /// </summary>
        public override sealed ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.CreateRange(new List<string> { 
            FileAnalyzerSettings.DiagnosticId,
            ClassAnalyzerSettings.DiagnosticId 
        });

        /// <summary>
        ///   Gets fix all provider.
        /// </summary>
        /// <returns> A FixAllProvider. </returns>
        public override sealed FixAllProvider GetFixAllProvider()
        {
            return FixAllProvider.Create(async (context, doc, codes) =>
            {
                Document d = context.Document;
                SyntaxNode root = await doc.GetSyntaxRootAsync(context.CancellationToken);          
                var classes = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>().ToArray();

                d = await ClassCodeFixProvider.AddDocumentationHeadersAsync(d, root, classes);

                SyntaxNode root1 = await d.GetSyntaxRootAsync(context.CancellationToken);
                return d.WithSyntaxRoot(root1);
            });
        }

        /// <summary>
        ///   Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: async (c) =>
                    {
                        Document d = context.Document;
                        SyntaxNode root = await d.GetSyntaxRootAsync(context.CancellationToken);
                        var classes = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>().ToArray();

                        d = await ClassCodeFixProvider.AddDocumentationHeadersAsync(d, root, classes);

                        SyntaxNode root1 = await d.GetSyntaxRootAsync(context.CancellationToken);
                        return d.WithSyntaxRoot(root1);
                    },
                    equivalenceKey: title),
                diagnostic);


        }

    }
}
