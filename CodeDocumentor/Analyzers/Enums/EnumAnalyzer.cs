﻿using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    /// <summary>
    ///   The enum analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnumAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   The title.
        /// </summary>
        private const string Title = "The enum must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        private const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        public const string DiagnosticId = Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID;

        /// <summary>
        ///   The message format.
        /// </summary>
        public const string MessageFormat = Title;

        /// <summary>
        ///   The diagnostic descriptor rule.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        ///   Initializes action.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.EnumDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        internal static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            EnumDeclarationSyntax node = context.Node as EnumDeclarationSyntax;
            if (node == null)
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

            context.ReportDiagnostic(Diagnostic.Create(Rule, node.Identifier.GetLocation()));
        }
    }
}
