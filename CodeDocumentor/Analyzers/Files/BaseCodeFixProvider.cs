using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor
{
    public abstract class BaseCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        ///  The title.
        /// </summary>
        protected const string FILE_FIX_TITLE = "Code Documentor this whole file";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        protected ImmutableArray<string> FileFixableDiagnosticIds => ImmutableArray.CreateRange(new List<string> {
            ClassAnalyzerSettings.DiagnosticId,
            PropertyAnalyzerSettings.DiagnosticId,
            ConstructorAnalyzerSettings.DiagnosticId,
            EnumAnalyzerSettings.DiagnosticId,
            InterfaceAnalyzerSettings.DiagnosticId,
            MethodAnalyzerSettings.DiagnosticId,
            FieldAnalyzerSettings.DiagnosticId,
            RecordAnalyzerSettings.DiagnosticId
        });

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        /// <summary>
        ///  Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        protected async Task RegisterFileCodeFixesAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            //#if DEBUG
            //            Debug.WriteLine("!!!DISABLING FILE CODE FIX. EITHER TESTS ARE RUNNING OR DEBUGGER IS ATTACHED!!!");
            //            return;
            //#endif
            //build it up, but check for counts if anything actually needs to be shown
            var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();
            var tempDoc = context.Document;
            var root = await tempDoc.GetSyntaxRootAsync(context.CancellationToken);
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
            neededCommentCount += RecordCodeFixProvider.BuildComments(root, _nodesTempToReplace);
            var newRoot = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) =>
            {
                return _nodesTempToReplace[n1];
            });
            if (neededCommentCount == 0)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: FILE_FIX_TITLE,
                    createChangedDocument: (c) => Task.Run(() => context.Document.WithSyntaxRoot(newRoot), c),
                    equivalenceKey: FILE_FIX_TITLE),
                diagnostic);
        }
    }
}
