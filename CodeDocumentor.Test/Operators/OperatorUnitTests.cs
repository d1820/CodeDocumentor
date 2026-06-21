using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Operators;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Operators
{
    public class OperatorUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public OperatorUnitTest(TestFixture fixture, ITestOutputHelper output)
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
        [InlineData("TestCode.cs", "TestFixCode.cs", 9, 47)]
        public async Task ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Operators/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Operators/TestFiles/{testCode}");

            var expected = new DiagnosticResult
            {
                Id = OperatorAnalyzerSettings.DiagnosticId,
                Message = OperatorAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);
            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OperatorCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            return new OperatorAnalyzer();
        }
    }
}
