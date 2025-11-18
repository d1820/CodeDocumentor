using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Classes;
using CodeDocumentor.Analyzers.Analyzers.Constructors;
using CodeDocumentor.Analyzers.Analyzers.Enums;
using CodeDocumentor.Analyzers.Analyzers.Fields;
using CodeDocumentor.Analyzers.Analyzers.Files;
using CodeDocumentor.Analyzers.Analyzers.Interfaces;
using CodeDocumentor.Analyzers.Analyzers.Methods;
using CodeDocumentor.Analyzers.Analyzers.Properties;
using CodeDocumentor.Analyzers.Analyzers.Records;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeDocumentor
{
    public abstract class BaseCodeFixProvider : CodeFixProvider
    {
        protected static IEventLogger EventLogger = ServiceLocator.Logger;

        /// <summary>
        ///  The title.
        /// </summary>
        protected const string FILE_FIX_TITLE = "Code Documentor this whole file";

        /// <summary>
        ///  Gets the fixable diagnostic ids.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.CreateRange(new List<string> {
            ClassAnalyzerSettings.DiagnosticId,
            PropertyAnalyzerSettings.DiagnosticId,
            ConstructorAnalyzerSettings.DiagnosticId,
            EnumAnalyzerSettings.DiagnosticId,
            InterfaceAnalyzerSettings.DiagnosticId,
            MethodAnalyzerSettings.DiagnosticId,
            FieldAnalyzerSettings.DiagnosticId,
            RecordAnalyzerSettings.DiagnosticId,
            FileAnalyzerSettings.DiagnosticId,
        });

        /// <summary>
        ///  Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        protected async Task RegisterFileCodeFixesAsync(CodeFixContext context, Diagnostic diagnostic)
        {
#if !DEBUG
            ServiceLocator.Logger.LogDebug(Constants.CATEGORY, "!!!DISABLING FILE CODE FIX. EITHER TESTS ARE RUNNING OR DEBUGGER IS ATTACHED!!!");
            return;
#else
            //build it up, but check for counts if anything actually needs to be shown
            var tempDoc = context.Document;
            var root = await tempDoc.GetSyntaxRootAsync(context.CancellationToken);
            if (root == null)
            {
                return;
            }
            var settings = await context.BuildSettingsAsync();
            var commentService = ServiceLocator.CommentBuilderService;
            TryHelper.Try(() =>
            {
                var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();

                //Order Matters
                var neededCommentCount = 0;
                neededCommentCount += commentService.BuildPropertyComments(settings, PropertyAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildConstructorComments(settings, ConstructorAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildEnumComments(settings, EnumAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildFieldComments(settings, FieldAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildMethodComments(settings, MethodAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                root = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) => _nodesTempToReplace[n1]);
                _nodesTempToReplace.Clear();
                neededCommentCount += commentService.BuildInterfaceComments(settings, InterfaceAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildComments(settings, ClassAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
                neededCommentCount += commentService.BuildRecordComments(settings, RecordAnalyzerSettings.DiagnosticId, root, _nodesTempToReplace);
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
#endif
        }
    }
}
