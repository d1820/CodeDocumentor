using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Files
{
    /// <summary>
    ///   The class analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///   Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(FileAnalyzerSettings.GetRule());
            }
        }

        /// <summary>
        ///   Initializes action.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.NamespaceDeclaration);
        }

        /// <summary>
        ///   Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            if (node == null)
            {
                return;
            }

            try
            {
                context.ReportDiagnostic(Diagnostic.Create(FileAnalyzerSettings.GetRule(), node.GetLocation()));
            }
            catch
            {
                //noop
            }
        }
    }
}
