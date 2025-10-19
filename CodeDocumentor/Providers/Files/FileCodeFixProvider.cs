using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers;
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
    public class FileCodeFixProvider : BaseCodeFixProvider
    {
        /// <summary>
        ///   The title.
        /// </summary>
        private const string Title = "Code Documentor this whole file";


        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.CreateRange(new List<string> {
            //ClassAnalyzerSettings.DiagnosticId,
            //PropertyAnalyzerSettings.DiagnosticId,
            //ConstructorAnalyzerSettings.DiagnosticId,
            //EnumAnalyzerSettings.DiagnosticId,
            //InterfaceAnalyzerSettings.DiagnosticId,
            //MethodAnalyzerSettings.DiagnosticId,
            //FieldAnalyzerSettings.DiagnosticId,
            //RecordAnalyzerSettings.DiagnosticId,
            FileAnalyzerSettings.DiagnosticId,
        });

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }


        /// <summary>
        ///   Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
//#if DEBUG
//            Debug.WriteLine("!!!DISABLING FILE CODE FIX. EITHER TESTS ARE RUNNING OR DEBUGGER IS ATTACHED!!!");
//            return;
//#endif
            Diagnostic diagnostic = context.Diagnostics.First();
            var settings = await context.BuildSettingsAsync(StaticSettings);
            //build it up, but check for counts if anything actually needs to be shown
            var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();
            Document tempDoc = context.Document;
            SyntaxNode root = await tempDoc.GetSyntaxRootAsync(context.CancellationToken);
            //Order Matters
            var neededCommentCount = 0;
            neededCommentCount += PropertyCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += ConstructorCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += EnumCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += FieldCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += MethodCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            root = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) =>
            {
                return _nodesTempToReplace[n1];
            });
            _nodesTempToReplace.Clear();
            neededCommentCount += InterfaceCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += ClassCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
            neededCommentCount += RecordCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
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
                    title: Title,
                    createChangedDocument: (c) => Task.Run(() => context.Document.WithSyntaxRoot(newRoot), c),
                    equivalenceKey: Title),
                diagnostic);
        }
    }
}
