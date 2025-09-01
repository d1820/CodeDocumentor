using System.Collections.Generic;
using System.Collections.Immutable;
#if DEBUG
#endif
using System.Threading.Tasks;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Helper;
using CodeDocumentor.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor
{

    public abstract class BaseCodeFixProvider : CodeFixProvider
    {
        protected DocumentationHeaderHelper DocumentationHeaderHelper = ServiceLocator.DocumentationHeaderHelper;

        protected static IEventLogger EventLogger = ServiceLocator.Logger;

        //expose this for some of the static helpers for producing ALl File comments
        private static ISettings _settings;

        public static void SetSettings(ISettings settings)
        {
            _settings = settings;
        }

        protected static ISettings StaticSettings =>
                //we serve up a fresh new instance from the static, and use that instead, keeps everything testable and decoupled from the static
                _settings?.Clone();

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
#if DEBUG
            //Debug.WriteLine("!!!DISABLING FILE CODE FIX. EITHER TESTS ARE RUNNING OR DEBUGGER IS ATTACHED!!!");
            //return;
#endif
            //build it up, but check for counts if anything actually needs to be shown
            var tempDoc = context.Document;
            var root = await tempDoc.GetSyntaxRootAsync(context.CancellationToken);
            if (root == null)
            {
                return;
            }
            var settings = await context.BuildSettingsAsync(StaticSettings);
            TryHelper.Try(() =>
            {
                var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();

                //Order Matters
                var neededCommentCount = 0;
                neededCommentCount += PropertyCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += ConstructorCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += EnumCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += FieldCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += MethodCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                root = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) => _nodesTempToReplace[n1]);
                _nodesTempToReplace.Clear();
                neededCommentCount += InterfaceCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += ClassCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                neededCommentCount += RecordCodeFixProvider.BuildComments(settings, root, _nodesTempToReplace);
                var newRoot = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) => _nodesTempToReplace[n1]);
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
            }, diagnostic.Id, EventLogger, eventId: Constants.EventIds.FILE_FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
        }
    }
}
