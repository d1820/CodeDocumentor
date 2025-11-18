using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CodeDocumentor2026.Executors
{
    public class TextSelectionExecutor
    {
        public void Execute(TextSelection textSelection, Func<string, string> selectionChangeCallback, int gotoLine = 1)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            textSelection.GotoLine(1, true);
            textSelection.SelectAll();
            var contents = textSelection.Text;
            var changedTxt = selectionChangeCallback.Invoke(contents);
            if (string.IsNullOrEmpty(changedTxt) || changedTxt == contents)
            {
                return;
            }
            textSelection.Insert(changedTxt);
            textSelection.SelectAll();
            textSelection.SmartFormat();
            textSelection.GotoLine(gotoLine, false);
        }
    }
}
