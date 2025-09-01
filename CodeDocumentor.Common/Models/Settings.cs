
using System.Collections.Generic;
using CodeDocumentor.Common.Interfaces;
using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Common.Models
{
    public class Settings : ISettings
    {
        public DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; } = DiagnosticSeverity.Warning;

        public DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        public DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether exclude asynchronously suffix.
        /// </summary>
        /// <value> A bool. </value>
        public bool ExcludeAsyncSuffix { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether include value node in properties.
        /// </summary>
        /// <value> A bool. </value>
        public bool IncludeValueNodeInProperties { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for publish members is only.
        /// </summary>
        /// <value> A bool. </value>
        public bool IsEnabledForPublicMembersOnly { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether enabled for non public is fields.
        /// </summary>
        public bool IsEnabledForNonPublicFields { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether preserve existing summary text.
        /// </summary>
        public bool PreserveExistingSummaryText { get; set; } = true;

        /// <summary>
        ///  Gets or Sets a value indicating whether use try and include crefs in method comments.
        /// </summary>
        /// <value> A bool. </value>
        public bool TryToIncludeCrefsForReturnTypes { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use natural language for return node.
        /// </summary>
        /// <value> A bool. </value>
        public bool UseNaturalLanguageForReturnNode { get; set; }

        /// <summary>
        ///  Gets or Sets a value indicating whether use to do comments on summary error.
        /// </summary>
        /// <value> A bool. </value>
        public bool UseToDoCommentsOnSummaryError { get; set; }

        /// <summary>
        ///  Gets or Sets the word maps.
        /// </summary>
        /// <value> An array of wordmaps. </value>
        public WordMap[] WordMaps { get; set; } = Constants.DEFAULT_WORD_MAPS;

        /// <summary>
        /// Gets or Sets a value indicating whether to use the .editorconfig file for settings.
        /// </summary>
        /// <remarks>
        /// This will convert the existing settings to a %USERPROFILE% .editorconfig file
        /// </remarks>
        public bool UseEditorConfigForSettings { get; set; }

        public ISettings Clone()
        {
            var newService = new Settings
            {
                ClassDiagnosticSeverity = ClassDiagnosticSeverity,
                ConstructorDiagnosticSeverity = ConstructorDiagnosticSeverity,
                DefaultDiagnosticSeverity = DefaultDiagnosticSeverity,
                EnumDiagnosticSeverity = EnumDiagnosticSeverity,
                ExcludeAsyncSuffix = ExcludeAsyncSuffix,
                FieldDiagnosticSeverity = FieldDiagnosticSeverity,
                IncludeValueNodeInProperties = IncludeValueNodeInProperties,
                InterfaceDiagnosticSeverity = InterfaceDiagnosticSeverity,
                IsEnabledForNonPublicFields = IsEnabledForNonPublicFields,
                IsEnabledForPublicMembersOnly = IsEnabledForPublicMembersOnly,
                MethodDiagnosticSeverity = MethodDiagnosticSeverity,
                PreserveExistingSummaryText = PreserveExistingSummaryText,
                PropertyDiagnosticSeverity = PropertyDiagnosticSeverity,
                RecordDiagnosticSeverity = RecordDiagnosticSeverity,
                TryToIncludeCrefsForReturnTypes = TryToIncludeCrefsForReturnTypes,
                UseNaturalLanguageForReturnNode = UseNaturalLanguageForReturnNode,
                UseToDoCommentsOnSummaryError = UseNaturalLanguageForReturnNode
            };
            var clonedMaps = new List<WordMap>();
            foreach (var item in WordMaps)
            {
                clonedMaps.Add(new WordMap
                {
                    Translation = item.Translation,
                    Word = item.Word,
                    WordEvaluator = item.WordEvaluator
                });
            }
            newService.WordMaps = clonedMaps.ToArray();
            return newService;
        }
    }
}
