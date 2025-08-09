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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicMethodAnalyzer : BaseDiagnosticAnalyzer
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
                    : ImmutableArray.Create(MethodAnalyzerSettings.GetRule());
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
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as MethodDeclarationSyntax;

            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            //NOTE [dturco 8.9.2025]:Since interfaces declarations do not have accessors, we do not need to check for public members only.
            var optionsService = OptionsService;
            if (node?.Parent.GetType() != typeof(InterfaceDeclarationSyntax) && optionsService.IsEnabledForPublicMembersOnly)
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
