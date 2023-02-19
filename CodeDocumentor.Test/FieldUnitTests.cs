using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class FieldUnitTest
    {
        /// <summary>
        /// The inherit doc test code.
        /// </summary>
        private const string InheritDocTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class FieldTester
	{
		/// <inheritdoc/>
		const int ConstFieldTester = 666;

		public FieldTester()
		{
		}
	}
}";

        /// <summary>
        /// The const field test code.
        /// </summary>
        private const string ConstFieldTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class FieldTester
	{
		public const int ConstFieldTester = 666;

		public FieldTester()
		{
		}
	}
}";

        /// <summary>
        /// The const field test fix code.
        /// </summary>
        private const string ConstFieldTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class FieldTester
	{
        /// <summary>
        /// The const field tester.
        /// </summary>
        public const int ConstFieldTester = 666;

		public FieldTester()
		{
		}
	}
}";
    }

    /// <summary>
    /// The field unit test.
    /// </summary>

    public partial class FieldUnitTest : CodeFixVerifier, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public FieldUnitTest(TestFixture fixture)
        {
            _fixture = fixture;
            DIContainer = fixture.DIContainer;
            _optionsService = fixture.OptionsService;
        }

        /// <summary>
        /// Nos diagnostics show.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        [Theory]
        [InlineData("")]
        [InlineData(InheritDocTestCode)]
        public void NoDiagnosticsShow(string testCode)
        {
            this.VerifyCSharpDiagnostic(testCode);
        }

        /// <summary>
        /// Shows diagnostic and fix.
        /// </summary>
        /// <param name="testCode">The test code.</param>
        /// <param name="fixCode">The fix code.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        [Theory]
        [InlineData(ConstFieldTestCode, ConstFieldTestFixCode, 10, 20)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var expected = new DiagnosticResult
            {
                Id = FieldAnalyzerSettings.DiagnosticId,
                Message = FieldAnalyzerSettings.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            this.VerifyCSharpDiagnostic(testCode, TestFixture.DIAG_TYPE_PUBLIC_ONLY, expected);

            this.VerifyCSharpFix(testCode, fixCode, TestFixture.DIAG_TYPE_PUBLIC_ONLY);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FieldCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            if (diagType == "private")
            {
                if (!BypassSettingPublicMembersOnly)
                {
                    _optionsService.IsEnabledForPublicMembersOnly = false;
                }
                return new NonPublicFieldAnalyzer();
            }
            if (diagType == TestFixture.DIAG_TYPE_PUBLIC_ONLY && !BypassSettingPublicMembersOnly)
            {
                _optionsService.IsEnabledForPublicMembersOnly = true;
            }
            return new FieldAnalyzer();
        }
    }
}
