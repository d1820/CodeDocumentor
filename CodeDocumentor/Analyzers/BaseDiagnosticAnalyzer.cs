using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    public abstract class BaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected DocumentationHeaderHelper DocumentationHeaderHelper;

#pragma warning disable IDE1006 // Naming Styles
        protected static IOptionsService _optionsService;
#pragma warning restore IDE1006 // Naming Styles

        public static void SetOptionsService(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        protected IOptionsService OptionsService =>
              //we serve up a fresh new instance from the static, and use that instead, keeps everything testable and decoupled from the static
              _optionsService.Clone();

    }
}
