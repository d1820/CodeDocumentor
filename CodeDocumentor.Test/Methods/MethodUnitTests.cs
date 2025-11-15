using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CodeDocumentor.Analyzers;
using CodeDocumentor.Analyzers.Methods;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Methods
{
    [SuppressMessage("XMLDocumentation", "")]
    public class MethodUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public MethodUnitTest(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            fixture.Initialize(output);
        }
        [Theory]
        [InlineData("")]
        [InlineData("InheritDocTestCode.cs")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            if (testCode?.Length == 0)
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

        [Theory]
        [InlineData("BasicTestCode", "BasicTestFixCode", 9, 21)]
        [InlineData("InterfacePrivateMethods", "InterfacePrivateMethodsFix", 9, 22)]
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
        [InlineData("MethodWithExceptionTestCode", "MethodWithExceptionTestFixCode", 9, 23)]
        [InlineData("MethodWithInlineExceptionTestCode", "MethodWithInlineExceptionTestFixCode", 9, 23)]
        [InlineData("MethodWithMixedExceptionTestCode", "MethodWithMixedExceptionTestFixCode", 9, 23)]
        [InlineData("MethodWithTaskAndComplexTypeReturnTypeTestCode", "MethodWithTaskAndComplexTypeReturnTypeTestFixCode", 9, 46)]
        [InlineData("MethodWithStartingVerbReturnTestCode", "MethodWithStartingVerbReturnTestFixCode", 9, 23)]
        public async Task ShowMethodDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Methods/TestFiles/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Methods/TestFiles/{testCode}.cs");
            _fixture.MockSettings.SetClone(new TestSettings
            {
                UseNaturalLanguageForReturnNode = false,
                TryToIncludeCrefsForReturnTypes = false
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

        [Theory]
        [InlineData("InterfacePrivateMethods", "InterfacePrivateMethodsFix", 9, 22)]
        public async Task ShowMethodDiagnosticAndFixForPrivate(string testCode, string fixCode, int line, int column)
        {
            var fix = _fixture.LoadTestFile($"./Methods/TestFiles/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Methods/TestFiles/{testCode}.cs");
            _fixture.MockSettings.SetClone(new TestSettings
            {
                UseNaturalLanguageForReturnNode = false,
                TryToIncludeCrefsForReturnTypes = false
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

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PRIVATE, expected);

            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PRIVATE);
        }


        [Theory]
        [InlineData("MethodWithNullableReturnTestCode", "MethodWithNullableReturnTestFixCode", 9, 30)]
        [InlineData("MethodWithNullableStringReturnTestCode", "MethodWithNullableStringReturnTestFixCode", 9, 24)]
        [InlineData("MethodWithStringReturnTestCode", "MethodWithStringReturnTestFixCode", 9, 23)]
        [InlineData("MethodWithReturnTestCode", "MethodWithReturnTestFixCode", 9, 29)]
        [InlineData("MethodWithObjectReturnTestCode", "MethodWithObjectReturnTestFixCode", 9, 23)]
        [InlineData("MethodWithIntReturnTestCode", "MethodWithIntReturnTestFixCode", 9, 20)]
        [InlineData("MethodWithCrefTestCode", "MethodWithCrefTestFixCode", 10, 35, false)]
        [InlineData("MethodWithCrefTestCodeNaturalLang", "MethodWithCrefTestFixCodeNaturalLang", 10, 35)]
        [InlineData("MethodWithExceptionTestCode", "MethodWithExceptionTestFixCode", 9, 23)]
        [InlineData("MethodWithInlineExceptionTestCode", "MethodWithInlineExceptionTestFixCode", 9, 23)]
        [InlineData("MethodWithMixedExceptionTestCode", "MethodWithMixedExceptionTestFixCode", 9, 23)]
        public async Task ShowMethodDiagnosticAndFixWhenCrefsIsTrue(string testCode, string fixCode, int line, int column, bool useNaturalLanguageForReturnNode = true)
        {
            var fix = _fixture.LoadTestFile($"./Methods/TestFiles/Crefs/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Methods/TestFiles/Crefs/{testCode}.cs");

            _fixture.MockSettings.SetClone(new TestSettings
            {
                UseNaturalLanguageForReturnNode = useNaturalLanguageForReturnNode,
                TryToIncludeCrefsForReturnTypes = true
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

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MethodCodeFixProvider();
        }

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
