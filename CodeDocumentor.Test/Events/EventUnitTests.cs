using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Events;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Events
{
    public class EventUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public EventUnitTest(TestFixture fixture, ITestOutputHelper output)
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
        [InlineData("EventFieldTestCode.cs", "EventFieldTestFixCode.cs", 9, 35)]
        [InlineData("ExplicitEventTestCode.cs", "ExplicitEventTestFixCode.cs", 9, 35)]
        public async Task ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Events/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Events/TestFiles/{testCode}");

            var expected = new DiagnosticResult
            {
                Id = EventAnalyzerSettings.DiagnosticId,
                Message = EventAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);
            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new EventCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            return new EventAnalyzer();
        }
    }
}
