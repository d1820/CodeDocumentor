using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Locators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeDocumentor.Analyzers.Fields
{
    /// <summary>
    ///  The field analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldAnalyzer : DiagnosticAnalyzer
    {
        private readonly FieldAnalyzerSettings _analyzerSettings;

        public FieldAnalyzer()
        {
            _analyzerSettings = new FieldAnalyzerSettings();
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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
        }

        /// <summary>
        ///  Analyzes node.
        /// </summary>
        /// <param name="context"> The context. </param>
        public void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is FieldDeclarationSyntax node))
            {
                return;
            }
            var settings = ServiceLocator.SettingService.BuildSettings(context);
            if (!settings.IsEnabledForNonPublicFields && PrivateMemberVerifier.IsPrivateMember(node))
            {
                return;
            }

            // Only const and static field.
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword) && !node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return;
            }
            var excludeAnanlyzer = ServiceLocator.DocumentationHeaderHelper.HasAnalyzerExclusion(node);
            if (excludeAnanlyzer)
            {
                return;
            }

            var field = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            context.BuildDiagnostic(node, field.Identifier, (alreadyHasComment) => _analyzerSettings.GetRule(alreadyHasComment, settings));
        }
    }
}
