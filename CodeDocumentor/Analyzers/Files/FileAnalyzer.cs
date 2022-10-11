﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeDocumentor.Helper;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.Package;

namespace CodeDocumentor
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
                return ImmutableArray.CreateRange(new List<DiagnosticDescriptor> {
                    FileAnalyzerSettings.GetRule(),
                    ClassAnalyzerSettings.GetRule() }
                );
            }
        }

        /// <summary>
        ///   Initializes action.
        /// </summary>
        /// <param name="context"> The context. </param>
        public override void Initialize(AnalysisContext context)
        {
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

            context.ReportDiagnostic(Diagnostic.Create(FileAnalyzerSettings.GetRule(), node.GetLocation()));

        }
    }
}