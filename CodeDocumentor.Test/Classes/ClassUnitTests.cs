using System.Threading.Tasks;
using CodeDocumentor.Analyzers.Analyzers.Classes;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Classes
{
    public class ClassUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public ClassUnitTest(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            fixture.Initialize(output);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ClassTesterInheritDoc.cs")]
        public async Task NoDiagnosticsShow(string testCode)
        {
            if (testCode == string.Empty)
            {
                await VerifyCSharpDiagnosticAsync(testCode, TestFixture.DIAG_TYPE_PUBLIC);
            }
            else
            {
                var file = _fixture.LoadTestFile($"./Classes/TestFiles/{testCode}");

                var expected = new DiagnosticResult
                {
                    Id = ClassAnalyzerSettings.DiagnosticId,
                    Message = ClassAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 4, 18)
                               }
                };

                await VerifyCSharpDiagnosticAsync(file, TestFixture.DIAG_TYPE_PUBLIC, expected);
            }
        }

        [Theory]
        [InlineData("ClassTester.cs", "ClassTesterFix.cs", 3, 19, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("PublicClassTester.cs", "PublicClassTesterFix.cs", 3, 26, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("ClassConstructorTester.cs", "ClassConstructorTesterFix.cs", 3, 18, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public async Task ShowClassDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($"./Classes/TestFiles/{fixCode}");
            var test = _fixture.LoadTestFile($"./Classes/TestFiles/{testCode}");

            var clone = new TestSettings();
            _fixture.SetPublicProcessingOption(clone, diagType);
            _fixture.MockSettings.SetClone(clone);

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

            await VerifyCSharpDiagnosticAsync(test, diagType, expected);

            await VerifyCSharpFixAsync(test, fix, diagType);
        }

        [Fact]
        public async Task SkipsClassDiagnosticAndFixWhenPublicOnlyTrue()
        {
            var fix = _fixture.LoadTestFile("./Classes/TestFiles/ClassTester.cs");
            var test = _fixture.LoadTestFile("./Classes/TestFiles/ClassTester.cs");
            var clone = new TestSettings
            {
                IsEnabledForPublicMembersOnly = true
            };
            _fixture.MockSettings.SetClone(clone);

            await VerifyCSharpDiagnosticAsync(test, TestFixture.DIAG_TYPE_PUBLIC_ONLY);

            await VerifyCSharpFixAsync(test, fix, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ClassCodeFixProvider();
        }

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
