using System.Collections.Immutable;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Analyzers.Destructors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DestructorAnalyzer : DiagnosticAnalyzer
    {
        private readonly DestructorAnalyzerSettings _analyzerSettings;

        public DestructorAnalyzer()
        {
            _analyzerSettings = new DestructorAnalyzerSettings();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.DestructorDeclaration);
        }

        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is DestructorDeclarationSyntax node))
            {
                return;
            }
            var excludeAnalyzer = ServiceLocator.DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnalyzer)
            {
                return;
            }
            var settings = ServiceLocator.SettingService.BuildSettings(context);
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
