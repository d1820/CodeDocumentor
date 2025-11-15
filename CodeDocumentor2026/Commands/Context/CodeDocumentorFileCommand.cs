using System;
using System.ComponentModel.Design;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor.Commands.Context
{
    /// <summary> Command handler </summary>
    internal sealed class CodeDocumentorFileCommand
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 25;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = CodeDocumentor.Common.Constants.CommandSetId;

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly SDTE _sdteService;
        private readonly IVsThreadedWaitDialogFactory _dialogFactory;
        private const string DIALOG_ACTION = "Attributing";

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeDocumentorFileCommand" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private CodeDocumentorFileCommand(AsyncPackage package, OleMenuCommandService commandService, SDTE SDTEService,
            IDataAnnoAttributeService attributeService, TextSelectionExecutor textSelectionExecutor,
            IVsThreadedWaitDialogFactory dialogFactory, SelectedItemCountExecutor selectedItemCountExecutor,
            AttributeExecutor attributeExecutor)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _sdteService = SDTEService;
            _attributeService = attributeService;
            _textSelectionExecutor = textSelectionExecutor;
            _dialogFactory = dialogFactory;
            _selectedItemCountExecutor = selectedItemCountExecutor;
            _attributeExecutor = attributeExecutor;
            var menuCommandID = new CommandID(_commandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorFileCommand Instance
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
            // Switch to the main thread - the call to AddCommand in ProtoCommand's constructor requires the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var attributeService = await package.GetServiceAsync(typeof(IDataAnnoAttributeService)) as IDataAnnoAttributeService;
            var dialogFactory = await package.GetServiceAsync(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
            var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
            var textSelectionExecutor = new TextSelectionExecutor();
            var selectedItemCountExecutor = new SelectedItemCountExecutor();
            var attributeExecutor = new AttributeExecutor();
            Instance = new CodeDocumentorFileCommand(package, commandService, SDTE, attributeService, textSelectionExecutor,
                dialogFactory, selectedItemCountExecutor, attributeExecutor);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = _sdteService as DTE;

            if (dte.SelectedItems.Count <= 0)
            {
                return;
            }

            var totalCount = _selectedItemCountExecutor.Execute(dte.SelectedItems);

            IVsThreadedWaitDialog2 dialog = null;
            if (totalCount > 1 && _dialogFactory != null)
            {
                //https://www.visualstudiogeeks.com/extensions/visualstudio/using-progress-dialog-in-visual-studio-extensions
                _dialogFactory.CreateInstance(out dialog);
            }

            var cts = new CancellationTokenSource();

            if (dialog == null ||
                dialog.StartWaitDialogWithPercentageProgress("Proto Attributor: Attributing Progress", "", $"0 of {totalCount} Processed",
                 null, DIALOG_ACTION, true, 0, totalCount, 0) != VSConstants.S_OK)
            {
                dialog = null;
            }

            try
            {
                _attributeExecutor.Execute(dte.SelectedItems, cts, dialog, totalCount, _textSelectionExecutor,
                   (content) => _attributeService.AddAttributes(content));
            }
            finally
            {
                dialog?.EndWaitDialog(out var usercancel);
            }
        }
    }
}
