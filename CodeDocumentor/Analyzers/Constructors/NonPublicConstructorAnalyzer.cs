using System.Collections.Generic;
using System.Collections.Immutable;
using CodeDocumentor.Builders;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPublicConstructorAnalyzer : BaseDiagnosticAnalyzer
    {
        private readonly ConstructorAnalyzerSettings _analyzerSettings;

        public NonPublicConstructorAnalyzer()
        {
            _analyzerSettings = new ConstructorAnalyzerSettings();
        }
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                //TODO: figure this out
                //var settings = StaticSettings;
                //return settings.IsEnabledForPublicMembersOnly
                //    ? new List<DiagnosticDescriptor>().ToImmutableArray()
                //    : ImmutableArray.Create(_analyzerSettings.GetRule());
                return ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());
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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConstructorDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as ConstructorDeclarationSyntax;
            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            var settings = BuildSettings(context, node);
            if (settings.IsEnabledForPublicMembersOnly)
            {
                return;
            }
            var excludeAnanlyzer = DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
