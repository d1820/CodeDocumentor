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
    ///   The method analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
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
            context.ReportDiagnostic(Diagnostic.Create(MethodAnalyzerSettings.GetRule(), node.Identifier.GetLocation()));
        }
    }
}
