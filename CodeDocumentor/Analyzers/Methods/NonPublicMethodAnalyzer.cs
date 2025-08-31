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
    public class NonPublicMethodAnalyzer : BaseDiagnosticAnalyzer
    {
        private readonly MethodAnalyzerSettings _analyzerSettings;

        public NonPublicMethodAnalyzer()
        {
            _analyzerSettings = new MethodAnalyzerSettings();
        }
        /// <summary>
        ///  Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                //var settings = StaticSettings;
                //return settings.IsEnabledForPublicMembersOnly
                //    ? new List<DiagnosticDescriptor>().ToImmutableArray()
                //    : ImmutableArray.Create(_analyzerSettings.GetSupportedDiagnosticRule());
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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node as MethodDeclarationSyntax;

            if (!PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }
            //NOTE: Since interfaces declarations do not have accessors, we allow documenting all the time.
            var settings = BuildSettings(context, node);
            if (!node.IsOwnedByInterface() && settings.IsEnabledForPublicMembersOnly)
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
