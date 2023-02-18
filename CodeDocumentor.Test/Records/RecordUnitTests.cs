using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test
{
    /// <summary>
    /// The class unit test.
    /// </summary>
    public class RecordUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;
        
        public RecordUnitTest(TestFixure fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// No diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        [InlineData("RecordTesterInheritDoc.cs")]
        public void NoDiagnosticsShow(string testCode)
        {
            if (testCode == string.Empty)
            {
                this.VerifyCSharpDiagnostic(testCode, "public");
            }
            else
            {
                var file = _fixture.LoadTestFile($@"./Records/TestFiles/{testCode}");

                var expected = new DiagnosticResult
                {
                    Id = RecordAnalyzerSettings.DiagnosticId,
                    Message = RecordAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 8, 19)
                               }
                };

                this.VerifyCSharpDiagnostic(file, "public", expected);
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
        [InlineData("RecordTester.cs", "RecordTesterFix.cs", 7, 20, TestFixure.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicRecordTester.cs", "PublicRecordTesterFix.cs", 7, 27, TestFixure.DIAG_TYPE_PUBLIC_ONLY)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($@"./Records/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($@"./Records/TestFiles/{testCode}");

            var expected = new DiagnosticResult
            {
                Id = RecordAnalyzerSettings.DiagnosticId,
                Message = RecordAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            this.VerifyCSharpDiagnostic(test, diagType, expected);

            this.VerifyCSharpFix(test, fix, diagType);
        }

        [Fact]
        public void SkipsDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile($@"./Records/TestFiles/RecordTester.cs");
            var test = _fixture.LoadTestFile($@"./Records/TestFiles/RecordTester.cs");
            _optionsService.IsEnabledForPublicMembersOnly = true;
            BypassSettingPublicMembersOnly = true;

            this.VerifyCSharpDiagnostic(test, TestFixure.DIAG_TYPE_PRIVATE);

            this.VerifyCSharpFix(test, fix, TestFixure.DIAG_TYPE_PRIVATE);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RecordCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == TestFixure.DIAG_TYPE_PRIVATE)
            {
                if (!BypassSettingPublicMembersOnly)
                {
                    _optionsService.IsEnabledForPublicMembersOnly = false;
                }
                return new NonPublicRecordAnalyzer();
            }
            if (diagType == TestFixure.DIAG_TYPE_PUBLIC_ONLY && !BypassSettingPublicMembersOnly)
            {
                _optionsService.IsEnabledForPublicMembersOnly = true;
            }
            return new RecordAnalyzer();
        }
    }
}
