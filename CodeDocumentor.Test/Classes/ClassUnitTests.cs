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
    public class ClassUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        
        public ClassUnitTest(TestFixture fixture)
        {
            _fixture = fixture;
            DIContainer = fixture.DIContainer;
            _optionsService = fixture.OptionsService;
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
                this.VerifyCSharpDiagnostic(testCode, "public");
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
        [InlineData("ClassTester.cs", "ClassTesterFix.cs", 7, 19, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicClassTester.cs", "PublicClassTesterFix.cs", 7, 26, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($@"./Classes/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($@"./Classes/TestFiles/{testCode}");

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

            this.VerifyCSharpDiagnostic(test, diagType, expected);

            this.VerifyCSharpFix(test, fix, diagType);
        }

        [Fact]
        public void SkipsDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile($@"./Classes/TestFiles/ClassTester.cs");
            var test = _fixture.LoadTestFile($@"./Classes/TestFiles/ClassTester.cs");
            _optionsService.IsEnabledForPublicMembersOnly = true;
            BypassSettingPublicMembersOnly = true;

            this.VerifyCSharpDiagnostic(test, TestFixture.DIAG_TYPE_PRIVATE);

            this.VerifyCSharpFix(test, fix, TestFixture.DIAG_TYPE_PRIVATE);
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
                if (!BypassSettingPublicMembersOnly)
                {
                    _optionsService.IsEnabledForPublicMembersOnly = false;
                }
                return new NonPublicClassAnalyzer();
            }
            if (diagType == TestFixture.DIAG_TYPE_PUBLIC_ONLY && !BypassSettingPublicMembersOnly)
            {
                _optionsService.IsEnabledForPublicMembersOnly = true;
            }
            return new ClassAnalyzer();
        }
    }
}
