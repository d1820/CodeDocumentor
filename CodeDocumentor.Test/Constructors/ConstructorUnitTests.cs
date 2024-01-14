using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Constructors
{
    /// <summary>
    /// The constructor unit test.
    /// </summary>
    [SuppressMessage("XMLDocumentation", "")]
    public class ConstrcutorUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public ConstrcutorUnitTest(TestFixture fixture, ITestOutputHelper output)
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
                var file = _fixture.LoadTestFile($"./Constructors/TestFiles/{testCode}");
                var expected = new DiagnosticResult
                {
                    Id = ConstructorAnalyzerSettings.DiagnosticId,
                    Message = ConstructorAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 16)
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
        [InlineData("PublicConstructorTestCode.cs", "PublicContructorTestFixCode.cs", 9, 16, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("PrivateConstructorTestCode.cs", "PrivateContructorTestFixCode.cs", 9, 17, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicConstructorWithBooleanParameterTestCode.cs", "PublicContructorWithBooleanParameterTestFixCode.cs", 9, 16, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public async Task ShowConstructorDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($"./Constructors/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Constructors/TestFiles/{testCode}");
            _fixture.RegisterCallback(_fixture.CurrentTestName, (o) =>
            {
                _fixture.SetPublicProcessingOption(o, diagType);
            });

            var expected = new DiagnosticResult
            {
                Id = ConstructorAnalyzerSettings.DiagnosticId,
                Message = ConstructorAnalyzerSettings.MessageFormat,
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
        public async Task SkipsConstructorDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile("./Constructors/TestFiles/PrivateConstructorTestCode.cs");
            var test = _fixture.LoadTestFile("./Constructors/TestFiles/PrivateConstructorTestCode.cs");
            _fixture.RegisterCallback(_fixture.CurrentTestName, (o) =>
            {
                o.IsEnabledForPublicMembersOnly = true;
            });

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY);

            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ConstructorCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == TestFixture.DIAG_TYPE_PRIVATE)
            {
                return new NonPublicConstructorAnalyzer();
            }
            return new ConstructorAnalyzer();
        }
    }
}
