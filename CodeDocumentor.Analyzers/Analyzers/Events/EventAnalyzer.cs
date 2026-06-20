using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Analyzers.Events
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EventAnalyzer : DiagnosticAnalyzer
    {
        private readonly EventAnalyzerSettings _analyzerSettings;

        public EventAnalyzer()
        {
            _analyzerSettings = new EventAnalyzerSettings();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeEventFieldNode, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEventNode, SyntaxKind.EventDeclaration);
        }

        public void AnalyzeEventFieldNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is EventFieldDeclarationSyntax node))
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
            var variable = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            context.BuildDiagnostic(node, variable.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }

        public void AnalyzeEventNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is EventDeclarationSyntax node))
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
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
