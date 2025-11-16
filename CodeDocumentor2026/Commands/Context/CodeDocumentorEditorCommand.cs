using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;

namespace CodeDocumentor2026.Commands.Context
{
    /// <summary> 
    /// Command handler for editor context menu to add documentation at cursor position
    /// </summary>
    internal sealed class CodeDocumentorEditorCommand
    {
        /// <summary> Editor context command ID. </summary>
        public const int EditorCommandId = 6014;

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
            var editorMenuItem = new OleMenuCommand(ExecuteAsync, editorCommandID);
            editorMenuItem.BeforeQueryStatus += OnBeforeQueryStatus;
            commandService.AddCommand(editorMenuItem);
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
                return FindDocumentableNode(documentInfo.Root, documentInfo.CursorPosition);
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
#pragma warning disable IDE1006 // Naming Styles
        private async void OnBeforeQueryStatus(object sender, EventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            var command = sender as OleMenuCommand;
            if (command == null)
            {
                return;
            }

            try
            {
                // Check if we can find a documentable node at the cursor position
                var targetNode = await GetSyntaxNodeAtCursorAsync();
                
                // Only show the menu item if we found a valid documentable node
                command.Visible = targetNode != null;
                command.Enabled = targetNode != null;
            }
            catch (Exception)
            {
                // If anything fails, hide the menu item
                command.Visible = false;
                command.Enabled = false;
            }
        }

        /// <summary>
        /// Executes the command when the editor context menu item is clicked
        /// </summary>
        private async void ExecuteAsync(object sender, EventArgs e)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                
                var documentInfo = await GetCurrentDocumentInfoAsync();
                if (documentInfo == null)
                {
                    return;
                }

                // Find documentable node at cursor position in THIS syntax tree
                var targetNode = FindDocumentableNode(documentInfo.Root, documentInfo.CursorPosition);
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

                var newRoot = documentInfo.Root.ReplaceNode(targetNode, newDeclaration);
                var updatedText = newRoot.ToFullString();

                // Update document
                if (updatedText != documentInfo.DocumentText)
                {
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

                    // Restore cursor position using the updated syntax tree
                    try
                    {
                        // Find the documented node in the new syntax tree
                        var updatedTargetNode = newRoot.DescendantNodes()
                            .FirstOrDefault(n => n.GetType() == targetNode.GetType() && 
                                                n.ToString().Trim() == newDeclaration.ToString().Trim());
                        
                        if (updatedTargetNode != null)
                        {
                            // Position cursor at the beginning of the documented node
                            var nodeStart = updatedTargetNode.GetLocation().SourceSpan.Start;
                            documentInfo.TextSelection.MoveToAbsoluteOffset(nodeStart + 1); // +1 for DTE 1-based indexing
                        }
                        else
                        {
                            // Fallback to original position calculation
                            documentInfo.TextSelection.MoveToLineAndOffset(documentInfo.OriginalLine, documentInfo.OriginalColumn);
                        }
                    }
                    catch
                    {
                        // If position restoration fails, just collapse at current position
                        documentInfo.TextSelection.Collapse();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] EditorCommand.Execute error: {ex}");
            }
        }

        /// <summary>
        /// Helper class to hold document information
        /// </summary>
        private class DocumentInfo
        {
            public SyntaxNode Root { get; set; }
            public int CursorPosition { get; set; }
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
                var root = syntaxTree.GetRoot();

                return new DocumentInfo
                {
                    Root = root,
                    CursorPosition = textSelection.ActivePoint.AbsoluteCharOffset - 1, // Convert from 1-based to 0-based
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
        private SyntaxNode FindDocumentableNode(SyntaxNode root, int position)
        {
            // Find the node at the exact cursor position
            var nodeAtPosition = root.FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(position, position));
            
            // Use the service to determine if it's documentable - don't traverse up
            if (_commentBuilderService.IsDocumentableNode(nodeAtPosition))
            {
                return nodeAtPosition;
            }
            
            return null;
        }
    }
}
