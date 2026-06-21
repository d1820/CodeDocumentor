using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Indexers;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Indexers
{
    public class IndexerUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public IndexerUnitTest(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            fixture.Initialize(output);
        }

        [Theory]
        [InlineData("")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            await VerifyCSharpDiagnosticAsync(testCode);
        }

        [Theory]
        [InlineData("TestCode.cs", "TestFixCode.cs", 9, 20)]
        [InlineData("WithSetterTestCode.cs", "WithSetterTestFixCode.cs", 10, 20)]
        public async Task ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Indexers/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Indexers/TestFiles/{testCode}");

            var expected = new DiagnosticResult
            {
                Id = IndexerAnalyzerSettings.DiagnosticId,
                Message = IndexerAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);
            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new IndexerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            return new IndexerAnalyzer();
        }
    }
}
