using System.Collections.Immutable;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    /// <summary> The class analyzer. </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassAnalyzer : DiagnosticAnalyzer
    {
        /// <summary> Gets the supported diagnostics. </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(ClassAnalyzerSettings.GetRule());
            }
        }

        /// <summary> Initializes action. </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        /// <summary> Analyzes node. </summary>
        /// <param name="context"> The context. </param>
        internal static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax node = context.Node as ClassDeclarationSyntax;
            if (node == null)
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

            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => ClassAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
