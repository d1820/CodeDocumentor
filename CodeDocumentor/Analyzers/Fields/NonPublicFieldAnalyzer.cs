using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
    public class NonPublicFieldAnalyzer : DiagnosticAnalyzer
    {
        /// <summary> Gets the supported diagnostics. </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
                if (optionsService.IsEnabledForPublicMembersOnly)
                {
                    return new List<DiagnosticDescriptor>().ToImmutableArray();
                }
                return ImmutableArray.Create(FieldAnalyzerSettings.GetRule());
            }
        }

        /// <summary> Initializes. </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
        }

        /// <summary> Analyzes node. </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            FieldDeclarationSyntax node = context.Node as FieldDeclarationSyntax;

            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            // Only const field.
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
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

            VariableDeclaratorSyntax field = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            context.BuildDiagnostic(node, field.Identifier, (alreadyHasComment) => FieldAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
