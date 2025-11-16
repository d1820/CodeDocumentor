using System;
using System.ComponentModel.Design;
using System.Threading;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor2026.Executors;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CodeDocumentor2026.Commands.Context
{
    /// <summary> 
    /// Unified command handler for both file and folder context menu items 
    /// </summary>
    internal sealed class CodeDocumentorContextCommand
    {
        /// <summary> File context command ID. </summary>
        public const int FileCommandId = 6013;
        
        /// <summary> Folder context command ID. </summary>
        public const int FolderCommandId = 6012;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = CodeDocumentor.Common.Constants.CommandSetId;

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly SDTE _sdteService;
        private readonly ICommentBuilderService _commentBuilderService;
        private readonly TextSelectionExecutor _textSelectionExecutor;
        private readonly IVsThreadedWaitDialogFactory _dialogFactory;
        private readonly SelectedItemCountExecutor _selectedItemCountExecutor;
        private readonly CommentExecutor _commentExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDocumentorContextCommand" /> class.
        /// Adds command handlers for both file and folder context menus.
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private CodeDocumentorContextCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService,
            ICommentBuilderService commentBuilderService, TextSelectionExecutor textSelectionExecutor,
            IVsThreadedWaitDialogFactory dialogFactory, SelectedItemCountExecutor selectedItemCountExecutor,
            CommentExecutor commentExecutor)
        {
            LogDebug("ContextCommand Constructor - START");
            
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _sdteService = SDTEService;
            _commentBuilderService = commentBuilderService;
            _textSelectionExecutor = textSelectionExecutor;
            _dialogFactory = dialogFactory;
            _selectedItemCountExecutor = selectedItemCountExecutor;
            _commentExecutor = commentExecutor;
            
            // Register both file and folder commands with the same handler
            var fileCommandID = new CommandID(_commandSet, FileCommandId);
            var folderCommandID = new CommandID(_commandSet, FolderCommandId);
            
            LogDebug($"ContextCommand Creating File MenuCommand with GUID: {_commandSet}, ID: {FileCommandId}");
            var fileMenuItem = new MenuCommand(Execute, fileCommandID);
            commandService.AddCommand(fileMenuItem);
            
            LogDebug($"ContextCommand Creating Folder MenuCommand with GUID: {_commandSet}, ID: {FolderCommandId}");
            var folderMenuItem = new MenuCommand(Execute, folderCommandID);
            commandService.AddCommand(folderMenuItem);
            
            LogDebug("ContextCommand Constructor - SUCCESS");
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorContextCommand Instance
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
                LogDebug("ContextCommand InitializeAsync - START");
                
                // Switch to the main thread - the call to AddCommand requires the UI thread.
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                LogDebug("ContextCommand Switched to main thread");

                LogDebug("ContextCommand Getting IMenuCommandService...");
                var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                LogDebug($"ContextCommand IMenuCommandService result: {(commandService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("ContextCommand Getting ICommentBuilderService...");
                var attributeService = await package.GetServiceAsync(typeof(ICommentBuilderService)) as ICommentBuilderService;
                LogDebug($"ContextCommand ICommentBuilderService result: {(attributeService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("ContextCommand Getting SVsThreadedWaitDialogFactory...");
                var dialogFactory = await package.GetServiceAsync(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                LogDebug($"ContextCommand SVsThreadedWaitDialogFactory result: {(dialogFactory != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("ContextCommand Getting SDTE service...");
                var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
                LogDebug($"ContextCommand SDTE service result: {(SDTE != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("ContextCommand Creating executor objects...");
                var textSelectionExecutor = new TextSelectionExecutor();
                var selectedItemCountExecutor = new SelectedItemCountExecutor();
                var commentExecutor = new CommentExecutor();
                LogDebug("ContextCommand Executor objects created");
                
                // Only create instance if required services are available
                if (commandService != null && attributeService != null && SDTE != null)
                {
                    LogDebug("ContextCommand All required services available - creating instance");
                    Instance = new CodeDocumentorContextCommand(package, commandService, SDTE, attributeService, textSelectionExecutor,
                        dialogFactory, selectedItemCountExecutor, commentExecutor);
                    LogDebug("ContextCommand InitializeAsync - SUCCESS");
                }
                else
                {
                    var errorMsg = "ContextCommand: Failed to get required services - " +
                        $"CommandService: {commandService != null}, CommentService: {attributeService != null}, SDTE: {SDTE != null}";
                    LogDebug($"ContextCommand InitializeAsync - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"ContextCommand initialization error: {ex}";
                LogDebug($"ContextCommand InitializeAsync - ERROR: {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                throw;
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when either context menu item is clicked.
        /// Handles both file and folder context menu items with the same logic.
        /// </summary>
        /// <param name="sender"> Event sender. </param>
        /// <param name="e"> Event args. </param>
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                LogDebug("ContextCommand Execute - START");
                ThreadHelper.ThrowIfNotOnUIThread();
                
                if (_sdteService == null || _commentBuilderService == null)
                {
                    var errorMsg = "ContextCommand.Execute: Required services not available - " +
                        $"SDTE: {_sdteService != null}, CommentService: {_commentBuilderService != null}";
                    LogDebug($"ContextCommand Execute - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                    return;
                }

                LogDebug("ContextCommand Getting DTE from SDTE service...");
                var dte = _sdteService as DTE;
                LogDebug($"ContextCommand DTE result: {(dte != null ? "SUCCESS" : "NULL")}");

                if (dte?.SelectedItems == null || dte.SelectedItems.Count <= 0)
                {
                    LogDebug("ContextCommand Execute - No selected items");
                    return;
                }

                LogDebug($"ContextCommand Selected items count: {dte.SelectedItems.Count}");
                var totalCount = _selectedItemCountExecutor.Execute(dte.SelectedItems);
                LogDebug($"ContextCommand Total count from executor: {totalCount}");

                IVsThreadedWaitDialog2 dialog = null;
                if (totalCount > 1 && _dialogFactory != null)
                {
                    LogDebug("ContextCommand Creating progress dialog...");
                    //https://www.visualstudiogeeks.com/extensions/visualstudio/using-progress-dialog-in-visual-studio-extensions
                    _dialogFactory.CreateInstance(out dialog);
                }

                var cts = new CancellationTokenSource();

                if (dialog == null ||
                    dialog.StartWaitDialogWithPercentageProgress("CodeDocumentor: Documenting Progress", "", $"0 of {totalCount} Processed",
                     null, CodeDocumentor2026.Constants.DIALOG_ACTION, true, 0, totalCount, 0) != VSConstants.S_OK)
                {
                    dialog = null;
                    LogDebug("ContextCommand Progress dialog not available or failed to start");
                }
                else
                {
                    LogDebug("ContextCommand Progress dialog started successfully");
                }

                try
                {
                    LogDebug("ContextCommand Starting comment executor...");
                    _commentExecutor.Execute(dte.SelectedItems, cts, dialog, totalCount, _textSelectionExecutor,
                       (content) => {
                           LogDebug($"ContextCommand Processing content length: {content?.Length ?? 0}");
                           var result = _commentBuilderService.AddDocumentation(content);
                           LogDebug($"ContextCommand Result content length: {result?.Length ?? 0}");
                           return result;
                       }, CodeDocumentor2026.Constants.DIALOG_ACTION);
                    LogDebug("ContextCommand Comment executor completed");
                }
                finally
                {
                    LogDebug("ContextCommand Ending progress dialog...");
                    var usercancel = 0;
                    dialog?.EndWaitDialog(out usercancel);
                    LogDebug($"ContextCommand Progress dialog ended, user canceled: {usercancel}");
                }
                
                LogDebug("ContextCommand Execute - SUCCESS");
            }
            catch (Exception ex)
            {
                var errorMsg = $"ContextCommand.Execute error: {ex}";
                LogDebug($"ContextCommand Execute - ERROR: {errorMsg}");
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
