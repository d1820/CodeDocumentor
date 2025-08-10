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
    ///  The method analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodAnalyzer : BaseDiagnosticAnalyzer
    {
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(MethodAnalyzerSettings.GetRule());
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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
            DocumentationHeaderHelper = new DocumentationHeaderHelper(OptionsService);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        internal void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax node))
            {
                return;
            }
            if (PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => MethodAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
