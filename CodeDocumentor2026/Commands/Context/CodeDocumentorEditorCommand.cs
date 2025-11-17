using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor2026.Extensions;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CodeDocumentor2026.Commands.Context
{
    /// <summary> 
    /// Command handler for editor context menu to add documentation at cursor position
    /// </summary>
    internal sealed class CodeDocumentorEditorCommand
    {
        /// <summary> Editor context command ID. </summary>
        public const int EditorCommandId = 6014;

        public const int EditorWholeFileCommandId = 6015;

        public const int QuickActionCommandId = 6017;

        public const int QuickActionWholeFileCommandId = 6018;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = CodeDocumentor.Common.Constants.CommandSetId;

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly ICommentBuilderService _commentBuilderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDocumentorEditorCommand" /> class.
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        /// <param name="commentBuilderService"></param>
        private CodeDocumentorEditorCommand(AsyncPackage package, OleMenuCommandService commandService,
            ICommentBuilderService commentBuilderService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _commentBuilderService = commentBuilderService;
            var editorCommandID = new CommandID(_commandSet, EditorCommandId);
            var editorMenuItem = new OleMenuCommand(Execute, editorCommandID);
            editorMenuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(editorMenuItem);

            var editorCommandWholeFileID = new CommandID(_commandSet, EditorWholeFileCommandId);
            var editorWholeFileMenuItem = new OleMenuCommand(Execute, editorCommandWholeFileID);
            commandService.AddCommand(editorWholeFileMenuItem);

            // New Quick Actions menu commands
            var quickActionCommandID = new CommandID(_commandSet, QuickActionCommandId);
            var quickActionMenuItem = new OleMenuCommand(Execute, quickActionCommandID);
            quickActionMenuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(quickActionMenuItem);

            var quickActionWholeFileCommandID = new CommandID(_commandSet, QuickActionWholeFileCommandId);
            var quickActionWholeFileMenuItem = new OleMenuCommand(Execute, quickActionWholeFileCommandID);
            commandService.AddCommand(quickActionWholeFileMenuItem);
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorEditorCommand Instance
        {
            get;
            private set;
        }

        /// <summary> Initializes the singleton instance of the command. </summary>
        /// <param name="package"> Owner package, not null. </param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                var attributeService = await package.GetServiceAsync(typeof(ICommentBuilderService)) as ICommentBuilderService;
                if (commandService != null && attributeService != null)
                {
                    Instance = new CodeDocumentorEditorCommand(package, commandService, attributeService);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"EditorCommand initialization error: {ex}";
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                throw;
            }
        }

        /// <summary>
        /// Gets the syntax node at the current cursor/right-click position using DTE
        /// </summary>
        /// <returns>The documentable syntax node at cursor position, or null if none found</returns>
        private async Task<SyntaxNode> GetSyntaxNodeAtCursorAsync()
        {
            try
            {
                var documentInfo = await GetCurrentDocumentInfoAsync();
                if (documentInfo == null)
                {
                    return null;
                }

                // Find documentable node at cursor position
                return FindDocumentableNode(documentInfo.Root, documentInfo.OriginalLine, documentInfo.OriginalColumn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] GetSyntaxNodeAtCursorAsync error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Called before the command is displayed to determine if it should be visible/enabled
        /// </summary>
        private async void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            command.Visible = false;
            command.Enabled = false;
            if (command == null)
            {

                return;
            }

            try
            {
                // Check if we can find a documentable node at the cursor position
                var targetNode = await GetSyntaxNodeAtCursorAsync();
                if (targetNode == null)
                {
                    return;
                }

                // Only show the menu item if we found a valid documentable node
                command.Visible = true;
                command.Enabled = true;
            }
            catch
            {
                //NO OPT
            }
        }

        /// <summary>
        /// Executes the command when the editor context menu item is clicked
        /// </summary>
        private async void Execute(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var documentInfo = await GetCurrentDocumentInfoAsync();
                if (documentInfo == null)
                {
                    return;
                }

                if (command.CommandID.ID == EditorWholeFileCommandId ||
                    command.CommandID.ID == QuickActionWholeFileCommandId)
                {
                    var documentedFile = _commentBuilderService.AddDocumentation(documentInfo.DocumentText);
                    UpdateDocumentAndFormat(documentInfo, documentedFile);
                    return;
                }


                // Find documentable node at cursor position in THIS syntax tree
                var targetNode = FindDocumentableNode(documentInfo.Root, documentInfo.OriginalLine, documentInfo.OriginalColumn);
                if (targetNode == null)
                {
                    return;
                }

                // Build new declaration and replace node
                var newDeclaration = _commentBuilderService.BuildNewDocumentationNode(targetNode);
                if (newDeclaration == null)
                {
                    return;
                }

                var commentLineCount = _commentBuilderService.GetDocumentationLineCount(newDeclaration);

                var newRoot = documentInfo.Root.ReplaceNode(targetNode, newDeclaration);
                var updatedText = newRoot.ToFullString();

                // Update document
                if (updatedText != documentInfo.DocumentText)
                {
                    UpdateDocumentAndFormat(documentInfo, updatedText);

                    documentInfo.TextSelection.SetCursorToLine(documentInfo.OriginalLine + commentLineCount, documentInfo.OriginalColumn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] EditorCommand.Execute error: {ex}");
            }
        }

        private static void UpdateDocumentAndFormat(DocumentInfo documentInfo, string updatedText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var editPoint = documentInfo.TextDocument.StartPoint.CreateEditPoint();
            editPoint.ReplaceText(
                documentInfo.TextDocument.EndPoint,
                updatedText,
                (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat
            );

            // Try to format the document after insertion
            try
            {
                editPoint.SmartFormat(documentInfo.TextDocument.StartPoint.CreateEditPoint());
            }
            catch
            {
                // If SmartFormat fails, continue without formatting
            }
        }

        /// <summary>
        /// Helper class to hold document information
        /// </summary>
        private class DocumentInfo
        {
            public SyntaxNode Root { get; set; }
            public string DocumentText { get; set; }
            public EnvDTE.TextDocument TextDocument { get; set; }
            public TextSelection TextSelection { get; set; }
            public int OriginalLine { get; set; }
            public int OriginalColumn { get; set; }
        }

        /// <summary>
        /// Gets current document information including syntax tree, cursor position, etc.
        /// </summary>
        private async Task<DocumentInfo> GetCurrentDocumentInfoAsync()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                // Get DTE service
                var dte = await _package.GetServiceAsync(typeof(SDTE)) as DTE;
                if (dte?.ActiveDocument == null)
                {
                    return null;
                }

                // Check if it's a C# file
                var activeDocument = dte.ActiveDocument;
                if (!activeDocument.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Get text selection to find cursor position
                var textSelection = activeDocument.Selection as TextSelection;
                if (textSelection == null)
                {
                    return null;
                }

                // Get the document text
                var textDocument = activeDocument.Object("TextDocument") as EnvDTE.TextDocument;
                if (textDocument == null)
                {
                    return null;
                }

                var startPoint = textDocument.StartPoint.CreateEditPoint();
                var documentText = startPoint.GetText(textDocument.EndPoint);

                // Parse with Roslyn
                var syntaxTree = CSharpSyntaxTree.ParseText(documentText);
                var root = await syntaxTree.GetRootAsync();

                return new DocumentInfo
                {
                    Root = root,
                    DocumentText = documentText,
                    TextDocument = textDocument,
                    TextSelection = textSelection,
                    OriginalLine = textSelection.ActivePoint.Line,
                    OriginalColumn = textSelection.ActivePoint.LineCharOffset
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] GetCurrentDocumentInfoAsync error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Finds a documentable syntax node at the specified position.
        /// Only returns a node if the cursor is directly on a documentable node - does not traverse up the tree.
        /// </summary>
        private SyntaxNode FindDocumentableNode(SyntaxNode root, int line, int column)
        {
            try
            {
                // Convert line/column to absolute position
                var sourceText = root.SyntaxTree.GetText();
                var position = sourceText.Lines[line - 1].Start + (column - 1); // Convert from 1-based to 0-based

                // Ensure position is within bounds
                if (position < 0 || position >= sourceText.Length)
                {
                    return null;
                }

                // Find the node at the exact cursor position
                var nodeAtPosition = root.FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(position, position));

                // Use the service to determine if it's documentable - don't traverse up
                if (_commentBuilderService.IsDocumentableNode(nodeAtPosition))
                {
                    return nodeAtPosition;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] FindDocumentableNode error: {ex}");
                return null;
            }
        }
    }
}
