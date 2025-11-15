using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CodeDocumentor2026.Executors
{
    public class TextSelectionExecutor
    {
        public void Execute(TextSelection textSelection, Func<string, string> seletionCallback)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            textSelection.GotoLine(1, true);
            textSelection.SelectAll();
            var contents = textSelection.Text;
            var changedTxt = seletionCallback.Invoke(contents);
            textSelection.Insert(changedTxt);
            textSelection.SmartFormat();
            textSelection.GotoLine(1, false);
        }
    }
}
