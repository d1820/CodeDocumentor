using System.Collections.Generic;
using System.Collections.Immutable;
using CodeDocumentor.Builders;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
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
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var optionsService = OptionsService;
                return optionsService.IsEnabledForPublicMembersOnly
                    ? new List<DiagnosticDescriptor>().ToImmutableArray()
                    : ImmutableArray.Create(PropertyAnalyzerSettings.GetRule());
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
        internal static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is PropertyDeclarationSyntax node))
            {
                return;
            }
            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var optionsService = OptionsService;
            if (optionsService.IsEnabledForPublicMembersOnly)
            {
                return;
            }

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => PropertyAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
