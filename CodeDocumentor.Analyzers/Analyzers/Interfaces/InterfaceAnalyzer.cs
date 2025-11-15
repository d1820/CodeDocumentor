using System.Collections.Immutable;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Analyzers.Interfaces
{
    /// <summary>
    ///  The interface analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InterfaceAnalyzer : DiagnosticAnalyzer
    {
        private readonly InterfaceAnalyzerSettings _analyzerSettings;

        public InterfaceAnalyzer()
        {
            _analyzerSettings = new InterfaceAnalyzerSettings();
        }
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());

        /// <summary>
        ///  Initializes action.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InterfaceDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InterfaceDeclarationSyntax node))
            {
                return;
            }
            var settings = ServiceLocator.SettingService.BuildSettings(context);
            if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var excludeAnanlyzer = ServiceLocator.DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
