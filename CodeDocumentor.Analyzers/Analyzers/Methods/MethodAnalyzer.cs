using System.Collections.Immutable;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Analyzers.Methods
{
    /// <summary>
    ///  The method analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodAnalyzer : DiagnosticAnalyzer
    {
        private readonly MethodAnalyzerSettings _analyzerSettings;

        public MethodAnalyzer()
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
        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MethodDeclarationSyntax node))
            {
                return;
            }

            //NOTE: Since interfaces declarations do not have accessors, we allow documenting all the time.
            var isPrivate = PrivateMemberVerifier.IsPrivateMember(node);
            var isOwnedByInterface = node.IsOwnedByInterface();
            if (isPrivate && !isOwnedByInterface)
            {
                return;
            }
            var excludeAnanlyzer = ServiceLocator.DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }
            var settings = ServiceLocator.SettingService.BuildSettings(context);
            context.BuildDiagnostic(node, node.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
