
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CodeDocumentor.Test.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Properties
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class PropertyUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public PropertyUnitTest(TestFixture fixture, ITestOutputHelper output)
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
                var file = _fixture.LoadTestFile($"./Properties/TestFiles/{testCode}");
                var expected = new DiagnosticResult
                {
                    Id = PropertyAnalyzerSettings.DiagnosticId,
                    Message = PropertyAnalyzerSettings.MessageFormat,
                    Severity = DiagnosticSeverity.Hidden,
                    Locations =
                         new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 23)
                               }
                };

                await VerifyCSharpDiagnosticAsync(file, TestFixture.DIAG_TYPE_PUBLIC, expected);
            }
        }

        [Theory]
        [InlineData("PropertyWithGetterSetterTestCode", "PropertyWithGetterSetterTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("PropertyOnlyGetterTestCode", "PropertyOnlyGetterTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("PropertyPrivateGetterTestCode", "PropertyPrivateGetterTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("PropertyInternalGetterTestCode", "PropertyInternalGetterTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("BooleanPropertyTestCode", "BooleanPropertyTestFixCode", 9, 21, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("BooleanPropertyTest2Code", "BooleanPropertyTest2FixCode", 9, 21, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("NullableBooleanPropertyTestCode", "NullableBooleanPropertyTestFixCode", 9, 22, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("ExpressionBodyPropertyTestCode", "ExpressionBodyPropertyTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("NullableDateTimePropertyTestCode", "NullableDateTimePropertyTestFixCode", 9, 26, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        [InlineData("PublicPropertyInterfaceTestCode", "PublicPropertyInterfaceTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC)]
        [InlineData("PrivatePropertyInterfaceTestCode", "PrivatePropertyInterfaceTestFixCode", 9, 16, TestFixture.DIAG_TYPE_PRIVATE)]
        [InlineData("TaskPropertyTestCode", "TaskPropertyTestFixCode", 9, 26, TestFixture.DIAG_TYPE_PUBLIC)]
        public async Task ShowPropertyDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
        {
            var fix = _fixture.LoadTestFile($"./Properties/TestFiles/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Properties/TestFiles/{testCode}.cs");
            var clone = new TestOptionsService();
            _fixture.SetPublicProcessingOption(clone, diagType);
            _fixture.MockOptionsService.SetClone(clone);
            var expected = new DiagnosticResult
            {
                Id = PropertyAnalyzerSettings.DiagnosticId,
                Message = PropertyAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, diagType, expected);

            await VerifyCSharpFixAsync(test, fix, diagType);
        }

        [Theory]
        [InlineData("PropertyWithGetterSetterValueNodeTestCode", "PropertyWithGetterSetterValueNodeTestFixCode", 9, 23, TestFixture.DIAG_TYPE_PUBLIC_ONLY, false)]
        [InlineData("PropertyWithGetterSetterValueNodeGenericTypeTestCode", "PropertyWithGetterSetterValueNodeGenericTypeTestFixCode", 9, 35, TestFixture.DIAG_TYPE_PUBLIC_ONLY, false)]
        [InlineData("PropertyWithGetterSetterValueNodeActionResultTypeTestCode", "PropertyWithGetterSetterValueNodeActionResultTypeTestFixCode", 9, 37, TestFixture.DIAG_TYPE_PUBLIC_ONLY)]
        public async Task ShowPropertyDiagnosticAndFixWithValueTypeEnabled(string testCode, string fixCode, int line, int column, string diagType, bool tryToIncludeCrefsForReturnTypes = true)
        {
            var fix = _fixture.LoadTestFile($"./Properties/TestFiles/{fixCode}.cs");
            var test = _fixture.LoadTestFile($"./Properties/TestFiles/{testCode}.cs");
            var clone = new TestOptionsService
            {
                IncludeValueNodeInProperties = true,
                TryToIncludeCrefsForReturnTypes = tryToIncludeCrefsForReturnTypes
            };
            _fixture.SetPublicProcessingOption(clone, diagType);
            _fixture.MockOptionsService.SetClone(clone);

            var expected = new DiagnosticResult
            {
                Id = PropertyAnalyzerSettings.DiagnosticId,
                Message = PropertyAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, diagType, expected);

            await VerifyCSharpFixAsync(test, fix, diagType);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PropertyCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == TestFixture.DIAG_TYPE_PRIVATE)
            {
                return new NonPublicPropertyAnalyzer();
            }
            return new PropertyAnalyzer();
        }
    }
}
