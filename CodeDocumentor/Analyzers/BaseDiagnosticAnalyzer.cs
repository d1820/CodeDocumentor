using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Helper;
using CodeDocumentor.Locators;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
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
