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
    /// <summary> Command handler </summary>
    internal sealed class CodeDocumentorFolderCommand
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 6011;

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
        ///     Initializes a new instance of the <see cref="CodeDocumentorFolderCommand" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private CodeDocumentorFolderCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService,
            ICommentBuilderService commentBuilderService, TextSelectionExecutor textSelectionExecutor,
            IVsThreadedWaitDialogFactory dialogFactory, SelectedItemCountExecutor selectedItemCountExecutor,
            CommentExecutor commentExecutor)
        {
            LogDebug("FolderCommand Constructor - START");
            
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _sdteService = SDTEService;
            _commentBuilderService = commentBuilderService;
            _textSelectionExecutor = textSelectionExecutor;
            _dialogFactory = dialogFactory;
            _selectedItemCountExecutor = selectedItemCountExecutor;
            _commentExecutor = commentExecutor;
            var menuCommandID = new CommandID(_commandSet, CommandId);
            
            LogDebug($"FolderCommand Creating MenuCommand with GUID: {_commandSet}, ID: {CommandId}");
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            
            LogDebug("FolderCommand Constructor - SUCCESS");
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorFolderCommand Instance
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
                LogDebug("FolderCommand InitializeAsync - START");
                
                // Switch to the main thread - the call to AddCommand in ProtoCommand's constructor requires the UI thread.
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
                LogDebug("FolderCommand Switched to main thread");

                LogDebug("FolderCommand Getting IMenuCommandService...");
                var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
                LogDebug($"FolderCommand IMenuCommandService result: {(commandService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FolderCommand Getting ICommentBuilderService...");
                var attributeService = await package.GetServiceAsync(typeof(ICommentBuilderService)) as ICommentBuilderService;
                LogDebug($"FolderCommand ICommentBuilderService result: {(attributeService != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FolderCommand Getting SVsThreadedWaitDialogFactory...");
                var dialogFactory = await package.GetServiceAsync(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
                LogDebug($"FolderCommand SVsThreadedWaitDialogFactory result: {(dialogFactory != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FolderCommand Getting SDTE service...");
                var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
                LogDebug($"FolderCommand SDTE service result: {(SDTE != null ? "SUCCESS" : "NULL")}");
                
                LogDebug("FolderCommand Creating executor objects...");
                var textSelectionExecutor = new TextSelectionExecutor();
                var selectedItemCountExecutor = new SelectedItemCountExecutor();
                var commentExecutor = new CommentExecutor();
                LogDebug("FolderCommand Executor objects created");
                
                // Only create instance if required services are available
                if (commandService != null && attributeService != null && SDTE != null)
                {
                    LogDebug("FolderCommand All required services available - creating instance");
                    Instance = new CodeDocumentorFolderCommand(package, commandService, SDTE, attributeService, textSelectionExecutor,
                        dialogFactory, selectedItemCountExecutor, commentExecutor);
                    LogDebug("FolderCommand InitializeAsync - SUCCESS");
                }
                else
                {
                    var errorMsg = "FolderCommand: Failed to get required services - " +
                        $"CommandService: {commandService != null}, CommentService: {attributeService != null}, SDTE: {SDTE != null}";
                    LogDebug($"FolderCommand InitializeAsync - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"FolderCommand initialization error: {ex}";
                LogDebug($"FolderCommand InitializeAsync - ERROR: {errorMsg}");
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
                LogDebug("FolderCommand Execute - START");
                ThreadHelper.ThrowIfNotOnUIThread();
                
                if (_sdteService == null || _commentBuilderService == null)
                {
                    var errorMsg = "FolderCommand.Execute: Required services not available - " +
                        $"SDTE: {_sdteService != null}, CommentService: {_commentBuilderService != null}";
                    LogDebug($"FolderCommand Execute - FAILED: {errorMsg}");
                    System.Diagnostics.Debug.WriteLine($"[CodeDocumentor2026] {errorMsg}");
                    return;
                }

                LogDebug("FolderCommand Getting DTE from SDTE service...");
                var dte = _sdteService as DTE;
                LogDebug($"FolderCommand DTE result: {(dte != null ? "SUCCESS" : "NULL")}");

                if (dte?.SelectedItems == null || dte.SelectedItems.Count <= 0)
                {
                    LogDebug("FolderCommand Execute - No selected items");
                    return;
                }

                LogDebug($"FolderCommand Selected items count: {dte.SelectedItems.Count}");
                var totalCount = _selectedItemCountExecutor.Execute(dte.SelectedItems);
                LogDebug($"FolderCommand Total count from executor: {totalCount}");

                IVsThreadedWaitDialog2 dialog = null;
                if (totalCount > 1 && _dialogFactory != null)
                {
                    LogDebug("FolderCommand Creating progress dialog...");
                    //https://www.visualstudiogeeks.com/extensions/visualstudio/using-progress-dialog-in-visual-studio-extensions
                    _dialogFactory.CreateInstance(out dialog);
                }

                var cts = new CancellationTokenSource();

                if (dialog == null ||
                    dialog.StartWaitDialogWithPercentageProgress("CodeDocumentor: Documenting Progress", "", $"0 of {totalCount} Processed",
                     null, Constants.DIALOG_ACTION, true, 0, totalCount, 0) != VSConstants.S_OK)
                {
                    dialog = null;
                    LogDebug("FolderCommand Progress dialog not available or failed to start");
                }
                else
                {
                    LogDebug("FolderCommand Progress dialog started successfully");
                }

                try
                {
                    LogDebug("FolderCommand Starting comment executor...");
                    _commentExecutor.Execute(dte.SelectedItems, cts, dialog, totalCount, _textSelectionExecutor,
                       (content) => {
                           LogDebug($"FolderCommand Processing content length: {content?.Length ?? 0}");
                           var result = _commentBuilderService.AddDocumentation(content);
                           LogDebug($"FolderCommand Result content length: {result?.Length ?? 0}");
                           return result;
                       });
                    LogDebug("FolderCommand Comment executor completed");
                }
                finally
                {
                    LogDebug("FolderCommand Ending progress dialog...");
                    var usercancel = 0;
                    dialog?.EndWaitDialog(out usercancel);
                    LogDebug($"FolderCommand Progress dialog ended, user canceled: {usercancel}");
                }
                
                LogDebug("FolderCommand Execute - SUCCESS");
            }
            catch (Exception ex)
            {
                var errorMsg = $"FolderCommand.Execute error: {ex}";
                LogDebug($"FolderCommand Execute - ERROR: {errorMsg}");
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
