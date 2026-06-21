using System.Collections.Immutable;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Analyzers.Indexers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IndexerAnalyzer : DiagnosticAnalyzer
    {
        private readonly IndexerAnalyzerSettings _analyzerSettings;

        public IndexerAnalyzer()
        {
            _analyzerSettings = new IndexerAnalyzerSettings();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IndexerDeclaration);
        }

        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is IndexerDeclarationSyntax node))
            {
                return;
            }
            var settings = ServiceLocator.SettingService.BuildSettings(context);
            if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var excludeAnalyzer = ServiceLocator.DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnalyzer)
            {
                return;
            }
            context.BuildDiagnostic(node, node.ThisKeyword, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
