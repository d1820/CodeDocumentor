using CodeDocumentor.Services;
using CodeDocumentor.Test.TestHelpers;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test.Classes
{
    /// <summary>
    /// The class unit test.
    /// </summary>
    public class ClassUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public ClassUnitTest(TestFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// No diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        [InlineData("ClassTesterInheritDoc.cs")]
        public void NoDiagnosticsShow(string testCode)
        {
            if (testCode == string.Empty)
            {
                VerifyCSharpDiagnostic(testCode, TestFixture.DIAG_TYPE_PUBLIC);
            }
            else
            {
                var file = _fixture.LoadTestFile($@"./Classes/TestFiles/{testCode}");

                var expected = new DiagnosticResult
                {
                    Id = ClassAnalyzerSettings.DiagnosticId,
                    Message = ClassAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 8, 18)
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
        [InlineData("ClassTester.cs", "ClassTesterFix.cs", 7, 19, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicClassTester.cs", "PublicClassTesterFix.cs", 7, 26, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($@"./Classes/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($@"./Classes/TestFiles/{testCode}");

            _fixture.OptionsPropertyCallback = (o) =>
            {
                _fixture.SetPublicProcessingOption(o, diagType);
            };
            var expected = new DiagnosticResult
            {
                Id = ClassAnalyzerSettings.DiagnosticId,
                Message = ClassAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            VerifyCSharpDiagnostic(test, diagType, expected);

            VerifyCSharpFix(test, fix, diagType);
        }

        [Fact]
        public void SkipsDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile($@"./Classes/TestFiles/ClassTester.cs");
            var test = _fixture.LoadTestFile($@"./Classes/TestFiles/ClassTester.cs");
            _fixture.OptionsPropertyCallback = (o) =>
            {
                o.IsEnabledForPublicMembersOnly = true;
            };

            VerifyCSharpDiagnostic(test, TestFixture.DIAG_TYPE_PRIVATE);

            VerifyCSharpFix(test, fix, TestFixture.DIAG_TYPE_PRIVATE);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ClassCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == TestFixture.DIAG_TYPE_PRIVATE)
            {
                return new NonPublicClassAnalyzer();
            }
            return new ClassAnalyzer();
        }
    }
}
