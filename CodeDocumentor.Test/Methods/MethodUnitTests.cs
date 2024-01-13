using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using CodeDocumentor.Test.TestHelpers;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Methods
{
    /// <summary>
    /// The method unit test.
    /// </summary>
    [SuppressMessage("XMLDocumentation", "")]
    public class MethodUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public MethodUnitTest(TestFixture fixture, ITestOutputHelper output)
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
        [InlineData("InheritDocTestCode.cs")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            if (testCode == string.Empty)
            {
                await VerifyCSharpDiagnosticAsync(testCode, TestFixture.DIAG_TYPE_PUBLIC);
            }
            else
            {
                var file = _fixture.LoadTestFile($"./Methods/TestFiles/{testCode}");
                var expected = new DiagnosticResult
                {
                    Id = MethodAnalyzerSettings.DiagnosticId,
                    Message = MethodAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 21)
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
        [InlineData("BasicTestCode", "BasicTestFixCode", 9, 21)]
        [InlineData("MethodWithParameterTestCode", "MethodWithParameterTestFixCode", 9, 21)]
        [InlineData("MethodWithBooleanParameterTestCode", "MethodWithBooleanParameterTestFixCode", 9, 21)]
        [InlineData("MethodWithNullableStructParameterTestCode", "MethodWithNullableStructParameterTestFixCode", 9, 21)]
        [InlineData("MethodWithReturnTestCode", "MethodWithReturnTestFixCode", 9, 29)]
        [InlineData("MethodWithStringReturnTestCode", "MethodWithStringReturnTestFixCode", 9, 23)]
        [InlineData("MethodWithObjectReturnTestCode", "MethodWithObjectReturnTestFixCode", 9, 23)]
        [InlineData("MethodWithIntReturnTestCode", "MethodWithIntReturnTestFixCode", 9, 20)]
        [InlineData("MethodWithListIntReturnTestCode", "MethodWithListIntReturnTestFixCode", 9, 26)]
        [InlineData("MethodWithListListIntReturnTestCode", "MethodWithListListIntReturnTestFixCode", 9, 32)]
        [InlineData("MethodWithListQualifiedNameReturnTestCode", "MethodWithListQualifiedNameReturnTestFixCode", 9, 26)]
        [InlineData("MethodWithCrefTestCode", "MethodWithCrefTestFixCode", 10, 35)]
        public async Task ShowMethodDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Methods/TestFiles/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Methods/TestFiles/{testCode}.cs");

            _fixture.RegisterCallback(nameof(ShowMethodDiagnosticAndFix), (o) =>
            {
                o.UseNaturalLanguageForReturnNode = false;
            });
            var expected = new DiagnosticResult
            {
                Id = MethodAnalyzerSettings.DiagnosticId,
                Message = MethodAnalyzerSettings.MessageFormat,
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
            return new MethodCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == "private")
            {
                return new NonPublicMethodAnalyzer();
            }
            return new MethodAnalyzer();
        }




    }
}
