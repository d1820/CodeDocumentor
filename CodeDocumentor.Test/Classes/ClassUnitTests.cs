using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    public partial class ClassUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ClassUnitTest(TestFixure fixture)
        {
            _fixture = fixture;
            TestFixture.BuildOptionsPageGrid();
            CodeDocumentorPackage.Options.DefaultDiagnosticSeverity = DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Nos diagnostics show.
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
                var file =  _fixture.LoadTestFile($@"./Classes/{testCode}");

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
        [InlineData("ClassTester.cs", "ClassTesterFix.cs", 7, 19, TestFixure.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicClassTester.cs", "PublicClassTesterFix.cs", 7, 26, TestFixure.DIAG_TYPE_PUBLIC_ONLY)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($@"./Classes/{fixCode}");
            var test = _fixture.LoadTestFile($@"./Classes/{testCode}");

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
            if (diagType == TestFixure.DIAG_TYPE_PRIVATE)
            {
                CodeDocumentorPackage.Options.IsEnabledForPublicMembersOnly = false;
                return new NonPublicClassAnalyzer();
            }
            if (diagType == TestFixure.DIAG_TYPE_PUBLIC_ONLY)
            {
                CodeDocumentorPackage.Options.IsEnabledForPublicMembersOnly = true;
            }
            return new ClassAnalyzer();
        }
    }
}
