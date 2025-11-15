using System;
using System.ComponentModel.Design;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor2026.Executors;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CodeDocumentor2026.Commands.Menu
{
    /// <summary> Command handler </summary>
    internal sealed class CodeDocumentorFileMenu
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 15;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = CodeDocumentor.Common.Constants.CommandSetId;

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly ICommentBuilderService _commentBuilderService;
        private readonly TextSelectionExecutor _textSelectionExecutor;
        private readonly SDTE _sdteService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeDocumentorFileMenu" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private CodeDocumentorFileMenu(AsyncPackage package, OleMenuCommandService commandService,
            SDTE SDTEService, ICommentBuilderService commentBuilderService, TextSelectionExecutor textSelectionExecutor)
        {
            LogDebug("FileMenu Constructor - START");
            
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _sdteService = SDTEService;
            _commentBuilderService = commentBuilderService;
            _textSelectionExecutor = textSelectionExecutor;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(_commandSet, CommandId);
            LogDebug($"FileMenu Creating MenuCommand with GUID: {_commandSet}, ID: {CommandId}");
            
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            
            LogDebug("FileMenu Constructor - SUCCESS");
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorFileMenu Instance
        {
            get;
            private set;
        }

        /// <summary> Gets the service provider from the owner package. </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return _package;
            }
        }

        /// <summary> Initializes the singleton instance of the command. </summary>
        /// <param name="package"> Owner package, not null. </param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            try
            {
                LogDebug("FileMenu InitializeAsync - START");
                
                // Switch to the main thread - the call to AddCommand in ProtoCommand's constructor requires the UI thread.
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                LogDebug("FileMenu Switched to main thread");

                LogDebug("FileMenu Getting IMenuCommandService...");
                var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                LogDebug($"FileMenu IMenuCommandService result: {(commandService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FileMenu Getting ICommentBuilderService...");
                var commentService = await package.GetServiceAsync(typeof(ICommentBuilderService)) as ICommentBuilderService;
                LogDebug($"FileMenu ICommentBuilderService result: {(commentService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FileMenu Getting SDTE service...");
                var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
                LogDebug($"FileMenu SDTE service result: {(SDTE != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FileMenu Creating TextSelectionExecutor...");
                var textSelectionExecutor = new TextSelectionExecutor();
                LogDebug("FileMenu TextSelectionExecutor created");
                
                // Only create instance if all services are available
                if (commandService != null && commentService != null && SDTE != null)
                {
                    LogDebug("FileMenu All services available - creating instance");
                    Instance = new CodeDocumentorFileMenu(package, commandService, SDTE, commentService, textSelectionExecutor);
                    LogDebug("FileMenu InitializeAsync - SUCCESS");
                }
                else
                {
                    var errorMsg = "FileMenu: Failed to get required services - " +
                        $"CommandService: {commandService != null}, CommentService: {commentService != null}, SDTE: {SDTE != null}";
                    LogDebug($"FileMenu InitializeAsync - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"FileMenu initialization error: {ex}";
                LogDebug($"FileMenu InitializeAsync - ERROR: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                throw;
            }
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked. See the
        ///     constructor to see how the menu item is associated with this function using OleMenuCommandService
        ///     service and MenuCommand class.
        /// </summary>
        /// <param name="sender"> Event sender. </param>
        /// <param name="e"> Event args. </param>
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                LogDebug("FileMenu Execute - START");
                ThreadHelper.ThrowIfNotOnUIThread();
                
                if (_sdteService == null || _commentBuilderService == null || _textSelectionExecutor == null)
                {
                    var errorMsg = "FileMenu.Execute: Required services not available - " +
                        $"SDTE: {_sdteService != null}, CommentService: {_commentBuilderService != null}, TextExecutor: {_textSelectionExecutor != null}";
                    LogDebug($"FileMenu Execute - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                    return;
                }

                LogDebug("FileMenu Getting DTE from SDTE service...");
                var dte = _sdteService as DTE;
                LogDebug($"FileMenu DTE result: {(dte != null ? "SUCCESS" : "NULL")}");
                
                if (dte?.ActiveDocument != null)
                {
                    LogDebug($"FileMenu Active document: {dte.ActiveDocument.Name}");
                    LogDebug("FileMenu Executing text selection processor...");
                    
                    _textSelectionExecutor.Execute((TextSelection)dte.ActiveDocument.Selection, 
                        (contents) => {
                            LogDebug($"FileMenu Processing content length: {contents?.Length ?? 0}");
                            var result = _commentBuilderService.AddDocumentation(contents);
                            LogDebug($"FileMenu Result content length: {result?.Length ?? 0}");
                            return result;
                        });
                    
                    LogDebug("FileMenu Execute - SUCCESS");
                }
                else
                {
                    LogDebug("FileMenu Execute - No active document");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"FileMenu.Execute error: {ex}";
                LogDebug($"FileMenu Execute - ERROR: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                // Don't re-throw to prevent VS crashes - just log the error
            }
        }
        
        private static void LogDebug(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {message}");
            }
            catch
            {
                // Don't let logging failures crash the extension
            }
        }
    }
}
