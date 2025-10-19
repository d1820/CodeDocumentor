using System;
using System.Linq;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Services
{
    public class PreLoadSettingService : ISettingService
    {
        public ISettings BuildSettings(AnalyzerConfigOptions options, ISettings settings)
        {
            return Settings.BuildDefaults();
        }

        public ISettings BuildSettings(SyntaxNodeAnalysisContext context, ISettings settings)
        {
            return Settings.BuildDefaults();
        }
    }
}
