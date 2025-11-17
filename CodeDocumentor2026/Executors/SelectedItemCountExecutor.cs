using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace CodeDocumentor2026.Executors
{

    public class SelectedItemCountExecutor
    {
        public int Execute(SelectedItems selectedItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectedItems == null)
            {
                return 0;
            }

            var totalCount = 0;
            foreach (SelectedItem selectedItem in selectedItems)
            {
                if (selectedItem.ProjectItem == null)
                {
                    continue;
                }
                GetTotalItemCount(selectedItem.ProjectItem, ref totalCount);
            }
            return totalCount;
        }

        private void GetTotalItemCount(ProjectItem projectItem, ref int count)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                var fullPath = projectItem.Properties.Item("FullPath")?.Value?.ToString();
                if (fullPath?.EndsWith(".cs") == true)
                {
                    count++;
                }
            }
            if (projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder && projectItem.ProjectItems.Count > 0)
            {
                foreach (ProjectItem item in projectItem.ProjectItems)
                {
                    GetTotalItemCount(item, ref count);
                }
            }
        }
    }
}
