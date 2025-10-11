using System;
using System.Linq;
using CodeDocumentor.Analyzers.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Builders
{
    public static class DiagnosticBuilder
    {
        /// <summary>
        ///  Builds the diagnostic.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <param name="node"> The node. </param>
        /// <param name="identifier"> The identifier. </param>
        /// <param name="getRuleCallback"> The get rule callback. </param>
        public static void BuildDiagnostic(this SyntaxNodeAnalysisContext context, CSharpSyntaxNode node,
                                            SyntaxToken identifier,
                                            Func<bool, DiagnosticDescriptor> getRuleCallback)
        {
            var commentTriviaSyntax = node
                .GetLeadingTrivia()
                .Select(o => o.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            var commentHelper = ServiceLocator.CommentHelper;
            var alreadyHasComment = commentTriviaSyntax != null && commentHelper.HasComment(commentTriviaSyntax);
            var settings = ServiceLocator.SettingService.BuildSettings(context, null);
            try
            {
                context.ReportDiagnostic(Diagnostic.Create(getRuleCallback.Invoke(alreadyHasComment), identifier.GetLocation()));
            }
            catch (OperationCanceledException)
            {
                //noop
            }
        }
    }
}
