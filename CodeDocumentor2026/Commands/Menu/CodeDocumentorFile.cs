using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ProtoAttributor.Commands.Menu
{
    /// <summary> Command handler </summary>
    internal sealed class CodeDocumentorFile
    {
        /// <summary> Command ID. </summary>
        public const int CommandId = 15;

        /// <summary> Command menu group (command set GUID). </summary>
        public static readonly Guid _commandSet = CodeDocumentor.Common.Constants.CommandSetId;

        /// <summary> VS Package that provides this command, not null. </summary>
        private readonly AsyncPackage _package;

        private readonly IDataAnnoAttributeService _attributeService;
        private readonly TextSelectionExecutor _textSelectionExecutor;
        private readonly SDTE _sdteService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeDocumentorFile" /> class. Adds our command handlers
        ///     for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package"> Owner package, not null. </param>
        /// <param name="commandService"> Command service to add command to, not null. </param>
        private CodeDocumentorFile(AsyncPackage package, OleMenuCommandService commandService,
            SDTE SDTEService, IDataAnnoAttributeService attributeService, TextSelectionExecutor textSelectionExecutor)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _attributeService = attributeService;
            _textSelectionExecutor = textSelectionExecutor;
            _sdteService = SDTEService;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(_commandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary> Gets the instance of the command. </summary>
        public static CodeDocumentorFile Instance
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
            var textSelectionExecutor = new TextSelectionExecutor();
            var SDTE = await package.GetServiceAsync(typeof(SDTE)) as SDTE;
            Instance = new CodeDocumentorFile(package, commandService, SDTE, attributeService, textSelectionExecutor);
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
            if (dte.ActiveDocument != null)
            {
                _textSelectionExecutor.Execute((TextSelection)dte.ActiveDocument.Selection, (contents) => _attributeService.AddAttributes(contents));
            }
        }
    }
}
