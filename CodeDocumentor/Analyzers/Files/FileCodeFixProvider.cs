using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

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
            EnumAnalyzerSettings.DiagnosticId,
            InterfaceAnalyzerSettings.DiagnosticId,
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
            //if (CodeDocumentorPackage.IsDebugMode)
            //{
            //    return;
            //}
            Diagnostic diagnostic = context.Diagnostics.First();

            //build it up, but check for counts if anything actually needs to be shown
            var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();
            Document tempDoc = context.Document;
            SyntaxNode root = await tempDoc.GetSyntaxRootAsync(context.CancellationToken);
            //Order Matters
            var neededCommentCount = 0;
            neededCommentCount += PropertyCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            neededCommentCount += ConstructorCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            neededCommentCount += EnumCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            neededCommentCount += FieldCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            neededCommentCount += MethodCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            root = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) =>
            {
                return _nodesTempToReplace[n1];
            });
            _nodesTempToReplace.Clear();
            neededCommentCount += InterfaceCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            neededCommentCount += ClassCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            var newRoot = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) =>
            {
                return _nodesTempToReplace[n1];
            });
            if(neededCommentCount == 0)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: async (c) =>
                    {
                        return context.Document.WithSyntaxRoot(newRoot);
                    },
                    equivalenceKey: title),
                diagnostic);


        }

    }
}
