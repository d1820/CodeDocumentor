using System.Collections.Immutable;
using CodeDocumentor.Builders;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    /// <summary>
    ///  The class analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicRecordAnalyzer : BaseDiagnosticAnalyzer
    {
        private readonly RecordAnalyzerSettings _analyzerSettings;

        public NonPublicRecordAnalyzer()
        {
            _analyzerSettings = new RecordAnalyzerSettings();
        }
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());
            }
        }

        /// <summary>
        ///  Initializes action.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.RecordDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as RecordDeclarationSyntax;
            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var settings = context.BuildSettings(StaticSettings);
            if (settings.IsEnabledForPublicMembersOnly)
            {
                return;
            }
            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment,settings));
        }
    }
}
