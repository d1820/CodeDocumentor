using System.Collections.Generic;
using System.Collections.Immutable;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicConstructorAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
                if (optionsService.IsEnabledForPublicMembersOnly)
                {
                    return new List<DiagnosticDescriptor>().ToImmutableArray();
                }
                return ImmutableArray.Create(ConstructorAnalyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///   Initializes.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConstructorDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            ConstructorDeclarationSyntax node = context.Node as ConstructorDeclarationSyntax;
            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (optionsService.IsEnabledForPublicMembersOnly)
            {
                return;
            }

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => ConstructorAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
