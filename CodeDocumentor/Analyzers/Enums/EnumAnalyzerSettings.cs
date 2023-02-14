﻿using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor
{
    internal static class EnumAnalyzerSettings
    {
        /// <summary>
        ///   The title.
        /// </summary>
        internal const string Title = "The enum must have a documentation header.";

        /// <summary>
        ///   The category.
        /// </summary>
        internal const string Category = DocumentationHeaderHelper.Category;

        /// <summary>
        ///   The diagnostic id.
        /// </summary>
        internal const string DiagnosticId = Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID;

        /// <summary>
        ///   The message format.
        /// </summary>
        internal const string MessageFormat = Title;

        /// <summary>
        ///   The diagnostic descriptor rule.
        /// </summary>
        internal static DiagnosticDescriptor GetRule(bool hideDiagnosticSeverity = false)
        {
            return new DiagnosticDescriptor(EnumAnalyzerSettings.DiagnosticId, EnumAnalyzerSettings.Title,
                EnumAnalyzerSettings.MessageFormat, EnumAnalyzerSettings.Category,
                hideDiagnosticSeverity ? DiagnosticSeverity.Hidden : CodeDocumentorPackage.Options?.DefaultDiagnosticSeverity ?? DiagnosticSeverity.Warning, true);
        }
    }
}