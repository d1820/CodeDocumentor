using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Common.Interfaces
{
    public interface ISettingService
    {
        ISettings BuildSettings(AnalyzerConfigOptions options, ISettings Settings);
        ISettings BuildSettings(SyntaxNodeAnalysisContext context, ISettings Settings);
    }
}
