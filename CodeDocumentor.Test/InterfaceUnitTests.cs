using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class InterfaceUnitTest
    {
        /// <summary>
        /// The test code.
        /// </summary>
        private const string TestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public interface IInterfaceTester
	{
	}
}";

        /// <summary>
        /// The test fix code.
        /// </summary>
        private const string TestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    /// <summary>
    /// The interface tester interface.
    /// </summary>
    public interface IInterfaceTester
	{
	}
}";
    }

    /// <summary>
    /// The interface unit test.
    /// </summary>
    
    public partial class InterfaceUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {

        private readonly TestFixure _fixture;

        public InterfaceUnitTest(TestFixure fixture)
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
        [InlineData(TestCode, TestFixCode, 8, 19)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
            var expected = new DiagnosticResult
            {
                Id = InterfaceAnalyzer.DiagnosticId,
                Message = InterfaceAnalyzer.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            this.VerifyCSharpDiagnostic(testCode, TestFixure.DIAG_TYPE_PUBLIC_ONLY, expected);

            this.VerifyCSharpFix(testCode, fixCode, TestFixure.DIAG_TYPE_PUBLIC_ONLY);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new InterfaceCodeFixProvider();
        }

        /// <summary>
        /// Gets c sharp diagnostic analyzer.
        /// </summary>
        /// <returns>A DiagnosticAnalyzer.</returns>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer(string diagType)
        {
            return new InterfaceAnalyzer();
        }
    }
}
