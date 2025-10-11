using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Locators;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers
{
    public abstract class BaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected DocumentationHeaderHelper DocumentationHeaderHelper = ServiceLocator.DocumentationHeaderHelper;

        private static ISettings _settings;

        public static void SetSettings(ISettings settings)
        {
            _settings = settings;
        }

        protected static ISettings StaticSettings =>
              //we serve up a fresh new instance from the static, and use that instead, keeps everything testable and decoupled from the static
              _settings?.Clone();
    }
}
