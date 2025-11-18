using System;
using System.Threading.Tasks;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Locators;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeDocumentor
{
    public static class EditorConfigExtensions
    {
        public static async Task<ISettings> BuildSettingsAsync(this CodeFixContext context)
        {
            try
            {
                var tree = await context.Document.GetSyntaxTreeAsync();
                var opts = context.Document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);
                var svc = ServiceLocator.SettingService;
                return svc?.BuildSettings(opts) ?? Settings.BuildDefaults();
            }
            catch (Exception ex)
            {
                ServiceLocator.Logger.LogDebug(Constants.CATEGORY, $"{nameof(BuildSettingsAsync)}: Returning StaticSettings: {ex.ToString()}");
                return Settings.BuildDefaults();
            }
        }
    }
}
