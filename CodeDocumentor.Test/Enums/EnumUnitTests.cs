using System.Threading.Tasks;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Enums
{
    /// <summary>
    /// The enum unit test.
    /// </summary>
    public class EnumUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public EnumUnitTest(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            fixture.Initialize(output);
        }

        /// <summary>
        /// Nos diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            await VerifyCSharpDiagnosticAsync(testCode);
        }

        /// <summary>
        /// Shows diagnostic and fix.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="fixCode">The fix code.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        [Theory]
        [InlineData("TestCode.cs", "TestFixCode.cs", 7, 17)]
        public async Task ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Enums/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Enums/TestFiles/{testCode}");
            var expected = new DiagnosticResult
            {
                Id = EnumAnalyzerSettings.DiagnosticId,
                Message = EnumAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);

            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new EnumCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            return new EnumAnalyzer();
        }
    }
}
