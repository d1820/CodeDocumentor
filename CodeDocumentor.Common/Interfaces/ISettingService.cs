using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Common.Interfaces
{
    public interface ISettingService
    {
        ISettings StaticSettings { get; set; }

        ISettings BuildSettings(AnalyzerConfigOptions options);
        ISettings BuildSettings(SyntaxNodeAnalysisContext context);
    }
}
