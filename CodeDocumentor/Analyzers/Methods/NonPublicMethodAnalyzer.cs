using System.Collections.Generic;
using System.Collections.Immutable;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (CodeDocumentorPackage.Options?.IsEnabledForPublicMembersOnly == true)
                {
                    return new List<DiagnosticDescriptor>().ToImmutableArray();
                }
                return ImmutableArray.Create(MethodAnalyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///   Initializes.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax node = context.Node as MethodDeclarationSyntax;

            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            if (CodeDocumentorPackage.Options?.IsEnabledForPublicMembersOnly == true)
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
