using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CodeDocumentor2026.Extensions
{
    public static class TestSelectionExtensions
    {
        public static void SetCursorToLine(this TextSelection textSelection, int line, int column)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Restore cursor position using the updated syntax tree
            try
            {
                textSelection.MoveToLineAndOffset(line, column);
            }
            catch
            {
                // If position restoration fails, just collapse at current position
                textSelection.Collapse();
            }
        }
    }
}
