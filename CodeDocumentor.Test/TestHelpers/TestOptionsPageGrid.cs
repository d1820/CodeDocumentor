using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public class TestOptionsPageGrid : IOptionPageGrid
    {
        public bool ExcludeAsyncSuffix { get; set; }

        public bool IncludeValueNodeInProperties { get; set; }

        public bool IsEnabledForPublishMembersOnly { get; set; }

        public bool UseNaturalLanguageForReturnNode { get; set; }

        public bool UseToDoCommentsOnSummaryError { get; set; }

        public WordMap[] WordMaps { get; set; } = Constants.WORD_MAPS;

        public DiagnosticSeverity DefaultDiagnosticSeverity { get; set; }
    }
}
