using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;

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

                // Get cursor position (convert from 1-based to 0-based)
                var cursorPosition = textSelection.ActivePoint.AbsoluteCharOffset - 1;

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

                // Find documentable node at cursor position
                return FindDocumentableNode(root, cursorPosition);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] GetSyntaxNodeAtCursorAsync error: {ex}");
                return null;
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
                
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] EditorCommand.Execute error: {ex}");
            }
        }

        /// <summary>
        /// Finds a documentable syntax node at or containing the specified position.
        /// Traverses up the syntax tree to find the first documentable node.
        /// </summary>
        private SyntaxNode FindDocumentableNode(SyntaxNode root, int position)
        {
            // Find the node at the exact cursor position
            var nodeAtPosition = root.FindNode(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(position, position));
            
            // Traverse up the syntax tree to find a documentable node
            var currentNode = nodeAtPosition;
            while (currentNode != null)
            {
                if (IsDocumentableNode(currentNode))
                {
                    return currentNode;
                }
                
                currentNode = currentNode.Parent;
            }
            
            return null;
        }

        /// <summary>
        /// Determines if a syntax node is documentable (can have XML documentation comments)
        /// </summary>
        private bool IsDocumentableNode(SyntaxNode node)
        {
            switch (node)
            {
                case ClassDeclarationSyntax _:
                case InterfaceDeclarationSyntax _:
                case RecordDeclarationSyntax _:
                case EnumDeclarationSyntax _:
                case MethodDeclarationSyntax _:
                case PropertyDeclarationSyntax _:
                case ConstructorDeclarationSyntax _:
                case FieldDeclarationSyntax _:

                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Applies documentation to a single specific syntax node using the same pattern as CodeFixProvider
        /// </summary>
        private SyntaxNode BuildNewDocumentationNode(SyntaxNode node)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classNode:
                    return _commentBuilderService.BuildNewDeclaration(classNode);
                case InterfaceDeclarationSyntax interfaceNode:
                    return _commentBuilderService.BuildNewDeclaration(interfaceNode);
                case RecordDeclarationSyntax recordNode:
                    return _commentBuilderService.BuildNewDeclaration(recordNode);
                case EnumDeclarationSyntax enumNode:
                    return _commentBuilderService.BuildNewDeclaration(enumNode);
                case MethodDeclarationSyntax methodNode:
                    return _commentBuilderService.BuildNewDeclaration(methodNode);
                case PropertyDeclarationSyntax propertyNode:
                    return _commentBuilderService.BuildNewDeclaration(propertyNode);
                case ConstructorDeclarationSyntax constructorNode:
                    return _commentBuilderService.BuildNewDeclaration(constructorNode);
                case FieldDeclarationSyntax fieldNode:
                    return _commentBuilderService.BuildNewDeclaration(fieldNode);
                default:
                    return null;
            }
        }
    }
}
