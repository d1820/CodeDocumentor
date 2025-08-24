using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    public abstract class BaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected DocumentationHeaderHelper DocumentationHeaderHelper;

        private static IOptionsService _optionsService;

        public static void SetOptionsService(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        protected IOptionsService GetOptionsService()
        {
            return OptionsService;
        }

        protected static IOptionsService OptionsService =>
              //we serve up a fresh new instance from the static, and use that instead, keeps everything testable and decoupled from the static
              _optionsService.Clone();

    }
}
