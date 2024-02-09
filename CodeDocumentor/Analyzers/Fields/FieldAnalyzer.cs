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
    /// <summary> The field analyzer. </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldAnalyzer : DiagnosticAnalyzer
    {
        /// <summary> Gets the supported diagnostics. </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
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
        internal static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as FieldDeclarationSyntax;
            if (node == null)
            {
                return;
            }
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            if (!optionsService.IsEnabledForNonPublicFields && PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            // Only const and static field.
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword) && !node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return;
            }

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            var field = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            context.BuildDiagnostic(node, field.Identifier, (alreadyHasComment) => FieldAnalyzerSettings.GetRule(alreadyHasComment));
        }
    }
}
