using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    /// <summary>
    ///   The field analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(FieldAnalyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///   Initializes.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        internal static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            FieldDeclarationSyntax node = context.Node as FieldDeclarationSyntax;
            if (node == null)
            {
                return;
            }
            if (PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            // Only const field.
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return;
            }

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            VariableDeclaratorSyntax field = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            context.BuildDiagnostic(node, field.Identifier, (alreadyHasComment) => FieldAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
