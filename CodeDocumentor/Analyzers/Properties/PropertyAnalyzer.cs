using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Helper;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{

    /// <summary>
    ///   The property analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PropertyAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(PropertyAnalyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///   Initializes.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            PropertyDeclarationSyntax node = context.Node as PropertyDeclarationSyntax;
            if (PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            DocumentationCommentTriviaSyntax commentTriviaSyntax = node
                .GetLeadingTrivia()
                .Select(o => o.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            if (commentTriviaSyntax != null && CommentHelper.HasComment(commentTriviaSyntax))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(PropertyAnalyzerSettings.GetRule(), node.Identifier.GetLocation()));
        }
    }
}
