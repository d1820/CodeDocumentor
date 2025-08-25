using System.Collections.Generic;
using System.Collections.Immutable;
using CodeDocumentor.Builders;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    /// <summary>
    ///  The property analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicPropertyAnalyzer : BaseDiagnosticAnalyzer
    {
        private PropertyAnalyzerSettings _analyzerSettings;

        public NonPublicPropertyAnalyzer()
        {
            _analyzerSettings = new PropertyAnalyzerSettings();
        }
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var settings = Settings;
                return settings.IsEnabledForPublicMembersOnly
                    ? new List<DiagnosticDescriptor>().ToImmutableArray()
                    : ImmutableArray.Create(_analyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///  Initializes.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        internal void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is PropertyDeclarationSyntax node))
            {
                return;
            }
            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var settings = Settings;
            if (settings.IsEnabledForPublicMembersOnly)
            {
                return;
            }
            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
