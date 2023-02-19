using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Test.TestHelpers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test.Fields
{
    /// <summary>
    /// The field unit test.
    /// </summary>
    public class FieldUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public FieldUnitTest(TestFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Nos diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        [InlineData("InheritDocTestCode.cs")]
        public void NoDiagnosticsShow(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);

            if (testCode == string.Empty)
            {
                VerifyCSharpDiagnostic(testCode, TestFixture.DIAG_TYPE_PUBLIC);
            }
            else
            {
                var file = _fixture.LoadTestFile($@"./Fields/TestFiles/{testCode}");

                var expected = new DiagnosticResult
                {
                    Id = FieldAnalyzerSettings.DiagnosticId,
                    Message = FieldAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 26)
                               }
                };

                VerifyCSharpDiagnostic(file, TestFixture.DIAG_TYPE_PUBLIC, expected);

            }
        }

        /// <summary>
        /// Shows diagnostic and fix.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="fixCode">The fix code.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        [Theory]
        [InlineData("ConstFieldTestCode.cs", "ConstFieldTestFixCode.cs", 9, 26)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($@"./Fields/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($@"./Fields/TestFiles/{testCode}");

            var expected = new DiagnosticResult
            {
                Id = FieldAnalyzerSettings.DiagnosticId,
                Message = FieldAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            VerifyCSharpDiagnostic(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);

            VerifyCSharpFix(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FieldCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == "private")
            {
                return new NonPublicFieldAnalyzer();
            }
            return new FieldAnalyzer();
        }
    }
}
