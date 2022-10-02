using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class ConstrcutorUnitTest {

        /// <summary>
        /// The inherit doc test code.
        /// </summary>
        private const string InheritDocTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
		/// <inheritdoc/>
		public ConstructorTester()
		{
		}
	}
}";

        /// <summary>
        /// The public constructor test code.
        /// </summary>
        private const string PublicConstructorTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
		public ConstructorTester()
		{
		}
	}
}";

        /// <summary>
        /// The public constructor test fix code.
        /// </summary>
        private const string PublicContructorTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
        /// <summary>
        /// Initializes a new instance of the <see cref=""ConstructorTester""/> class.
        /// </summary>
        public ConstructorTester()
		{
		}
	}
}";

        /// <summary>
        /// The private constructor test code.
        /// </summary>
        private const string PrivateConstructorTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
		private ConstructorTester()
		{
		}
	}
}";

        /// <summary>
        /// The private constructor test fix code.
        /// </summary>
        private const string PrivateContructorTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
        /// <summary>
        /// Prevents a default instance of the <see cref=""ConstructorTester""/> class from being created.
        /// </summary>
        private ConstructorTester()
		{
		}
	}
}";

        /// <summary>
        /// The public constructor test code.
        /// </summary>
        private const string PublicConstructorWithBooleanParameterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
		public ConstructorTester(bool isRed, bool? isAssociatedWithAllProduct)
		{
		}
	}
}";

        /// <summary>
        /// The public contructor test fix code.
        /// </summary>
        private const string PublicContructorWithBooleanParameterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class ConstructorTester
	{
        /// <summary>
        /// Initializes a new instance of the <see cref=""ConstructorTester""/> class.
        /// </summary>
        /// <param name=""isRed"">If true, is red.</param>
        /// <param name=""isAssociatedWithAllProduct"">If true, is associated with all product.</param>
        public ConstructorTester(bool isRed, bool? isAssociatedWithAllProduct)
		{
		}
	}
}";
    }
    /// <summary>
    /// The constructor unit test.
    /// </summary>
    
    [SuppressMessage("XMLDocumentation", "")]
    public partial class ConstrcutorUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public ConstrcutorUnitTest(TestFixure fixture)
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
		[InlineData(PublicConstructorTestCode, PublicContructorTestFixCode, 10, 10, TestFixure.DIAG_TYPE_PUBLIC_ONLY)]
		[InlineData(PrivateConstructorTestCode, PrivateContructorTestFixCode, 10, 11, TestFixure.DIAG_TYPE_PRIVATE)]
		[InlineData(PublicConstructorWithBooleanParameterTestCode, PublicContructorWithBooleanParameterTestFixCode, 10, 10, TestFixure.DIAG_TYPE_PUBLIC_ONLY)]
		public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column, string diagType)
		{
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

			this.VerifyCSharpDiagnostic(testCode, diagType, expected);

			this.VerifyCSharpFix(testCode, fixCode, diagType);
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
            if (diagType == "private")
            {
                CodeDocumentorPackage.Options.IsEnabledForPublishMembersOnly = false;
                return new NonPublicConstructorAnalyzer();
            }
            if (diagType == TestFixure.DIAG_TYPE_PUBLIC_ONLY)
            {
                CodeDocumentorPackage.Options.IsEnabledForPublishMembersOnly = true;
            }
            return new ConstructorAnalyzer();
		}
	}
}
