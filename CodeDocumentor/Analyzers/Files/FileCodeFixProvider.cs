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
            ClassAnalyzerSettings.DiagnosticId,
            PropertyAnalyzerSettings.DiagnosticId,
            ConstructorAnalyzerSettings.DiagnosticId,
            EnumAnalyzer.DiagnosticId,
            InterfaceAnalyzer.DiagnosticId,
            MethodAnalyzerSettings.DiagnosticId,
            FieldAnalyzerSettings.DiagnosticId
        });

        /// <summary>
        ///   Gets fix all provider.
        /// </summary>
        /// <returns> A FixAllProvider. </returns>
        public override sealed FixAllProvider GetFixAllProvider()
        {
            return null;
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
                        var _nodesToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();
                        Document d = context.Document;
                        SyntaxNode root = await d.GetSyntaxRootAsync(context.CancellationToken);
                        ClassCodeFixProvider.BuildComments(root, _nodesToReplace);
                        PropertyCodeFixProvider.BuildComments(root, _nodesToReplace);
                        ConstructorCodeFixProvider.BuildComments(root, _nodesToReplace);
                        EnumCodeFixProvider.BuildComments(root, _nodesToReplace);
                        FieldCodeFixProvider.BuildComments(root, _nodesToReplace);
                        InterfaceCodeFixProvider.BuildComments(root, _nodesToReplace);
                        MethodCodeFixProvider.BuildComments(root, _nodesToReplace);

                        var newroot = root.ReplaceNodes(_nodesToReplace.Keys, (n1, n2) =>
                        {
                            return _nodesToReplace[n1];
                        });
                        return d.WithSyntaxRoot(newroot);
                    },
                    equivalenceKey: title),
                diagnostic);


        }

    }
}
