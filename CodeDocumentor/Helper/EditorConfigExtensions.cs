using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Locators;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeDocumentor
{
    public static class EditorConfigExtensions
    {
        public static async Task<ISettings> BuildSettingsAsync(this CodeFixContext context, ISettings Settings)
        {
            try
            {
                var tree = await context.Document.GetSyntaxTreeAsync();
                var opts = context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);
                var svc = ServiceLocator.SettingService;
                return svc.BuildSettings(opts, Settings);
            }
            catch
            {
                return Settings;
            }
        }
    }
}
