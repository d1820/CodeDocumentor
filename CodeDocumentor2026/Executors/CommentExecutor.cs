using System;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodeDocumentor2026.Executors
{

    public class CommentExecutor
    {

        public void Execute(SelectedItems selectedItems, CancellationTokenSource cts,
                            IVsThreadedWaitDialog2 dialog, int totalCount,
                            TextSelectionExecutor textSelectionExecutor,
                            Func<string, string> projectItemApplyAttributing,
                            string dialogAction = Constants.DIALOG_ACTION)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if(selectedItems == null)
            {
                return;
            }

            var currentCount = 0;
            bool cancelProcessing = false;

            foreach (SelectedItem selectedItem in selectedItems)
            {
                dialog?.HasCanceled(out cancelProcessing);
                if (cancelProcessing)
                {
                    cts.Cancel();
                    break;
                }
                if (selectedItem.ProjectItem == null)
                {
                    continue;
                }
                Action<string> projectItemAttributingComplete = (fileName) => {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    currentCount++;
                    dialog?.UpdateProgress($"{dialogAction}: {fileName}", $"{currentCount} of {totalCount} Processed", dialogAction, currentCount, totalCount, false, out cancelProcessing);
                    if (cancelProcessing)
                    {
                        cts.Cancel();
                    }
                };

                Action<string> projectItemAttributingStarted = (fileName) => {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    dialog?.UpdateProgress($"{dialogAction}: {fileName}", $"{currentCount} of {totalCount} Processed", dialogAction, currentCount, totalCount, false, out cancelProcessing);
                    if (cancelProcessing)
                    {
                        cts.Cancel();
                    }
                };

                ProcessProjectItem(selectedItem.ProjectItem, cts.Token,
                                        textSelectionExecutor,
                                        projectItemAttributingComplete,
                                        projectItemAttributingStarted,
                                        projectItemApplyAttributing);
            }
        }

        private void ProcessProjectItem(ProjectItem projectItem, CancellationToken token,
                                        TextSelectionExecutor textSelectionExecutor,
                                        Action<string> projectItemAttributingComplete,
                                        Action<string> projectItemAttributingStarted,
                                        Func<string, string> projectItemApplyAttributing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (token.IsCancellationRequested)
            {
                return;
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
            {
                if (projectItem.ProjectItems.Count > 0)
                {
                    foreach (ProjectItem item in projectItem.ProjectItems)
                    {
                        ProcessProjectItem(item, token, textSelectionExecutor, projectItemAttributingComplete, projectItemAttributingStarted, projectItemApplyAttributing);
                    }
                }
                return;
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                var fullPath = projectItem.Properties.Item("FullPath")?.Value?.ToString();
                var name = projectItem.Name;
                projectItemAttributingStarted?.Invoke(name);
                var isOpen = projectItem.IsOpen[EnvDTE.Constants.vsViewKindTextView];
                if (!isOpen)
                {
                    if (fullPath?.EndsWith(".cs") == true)
                    {
                        var window = projectItem.Open(EnvDTE.Constants.vsViewKindTextView);
                        window.Activate();
                        //process file
                        if (projectItem.Document != null)
                        {
                            projectItem.Document.Activate();
                            textSelectionExecutor.Execute((TextSelection)projectItem.Document.Selection, (contents) => projectItemApplyAttributing.Invoke(contents));
                        }
                        projectItemAttributingComplete?.Invoke(name);
                    }
                }
                else if (fullPath?.EndsWith(".cs") == true)
                {
                    //process file
                    if (projectItem.Document != null)
                    {
                        projectItem.Document.Activate();
                        textSelectionExecutor.Execute((TextSelection)projectItem.Document.Selection, (contents) => projectItemApplyAttributing.Invoke(contents));
                    }
                    projectItemAttributingComplete?.Invoke(name);
                }
            }
        }
    }
}
