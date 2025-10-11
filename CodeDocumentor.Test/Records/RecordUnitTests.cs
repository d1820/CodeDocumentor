using System.Threading.Tasks;
using CodeDocumentor.Analyzers;
using CodeDocumentor.Analyzers.Records;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Records
{
    /// <summary>
    /// The class unit test.
    /// </summary>
    public class RecordUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public RecordUnitTest(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            fixture.Initialize(output);
        }

        /// <summary>
        /// No diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        [InlineData("RecordTesterInheritDoc.cs")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            if (testCode == string.Empty)
            {
                await VerifyCSharpDiagnosticAsync(testCode, TestFixture.DIAG_TYPE_PUBLIC);
            }
            else
            {
                var file = _fixture.LoadTestFile($"./Records/TestFiles/{testCode}");

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

                await VerifyCSharpDiagnosticAsync(file, TestFixture.DIAG_TYPE_PUBLIC, expected);
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
        [InlineData("RecordTester.cs", "RecordTesterFix.cs", 7, 20, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicRecordTester.cs", "PublicRecordTesterFix.cs", 7, 27, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public async Task ShowRecordDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($"./Records/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Records/TestFiles/{testCode}");
            var clone = new TestSettings();
            _fixture.SetPublicProcessingOption(clone, diagType);
            _fixture.MockSettings.SetClone(clone);

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

            await VerifyCSharpDiagnosticAsync(test, diagType, expected);

            await VerifyCSharpFixAsync(test, fix, diagType);
        }

        [Fact]
        public async Task SkipsRecordDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile("./Records/TestFiles/RecordTester.cs");
            var test = _fixture.LoadTestFile("./Records/TestFiles/RecordTester.cs");
            var clone = new TestSettings {
                IsEnabledForPublicMembersOnly = true
            };
            _fixture.MockSettings.SetClone(clone);

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY);

            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
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
            if (diagType == TestFixture.DIAG_TYPE_PRIVATE)
            {
                return new NonPublicRecordAnalyzer();
            }
            return new RecordAnalyzer();
        }
    }
}
