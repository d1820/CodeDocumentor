using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Services
{
    public class PreLoadSettingService : ISettingService
    {
        public ISettings StaticSettings { get; set; }

        public ISettings BuildSettings(AnalyzerConfigOptions options)
        {
            return Settings.BuildDefaults();
        }

        public ISettings BuildSettings(SyntaxNodeAnalysisContext context)
        {
            return Settings.BuildDefaults();
        }
    }
}
