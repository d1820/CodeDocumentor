using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class PropertyUnitTest
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
	public class PropertyTester
	{
		/// <inheritdoc/>
		public string PersonName { get; set; }
	}
}";

        /// <summary>
        /// The property with getter setter test code.
        /// </summary>
        private const string PropertyWithGetterSetterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public string PersonName { get; set; }
	}
}";

        /// <summary>
        /// The property with getter setter test fix code.
        /// </summary>
        private const string PropertyWithGetterSetterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets or Sets the person name.
        /// </summary>
        public string PersonName { get; set; }
	}
}";

        /// <summary>
        /// The property only getter test code.
        /// </summary>
        private const string PropertyOnlyGetterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public string PersonName { get; }
	}
}";

        /// <summary>
        /// The property only getter test fix code.
        /// </summary>
        private const string PropertyOnlyGetterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets the person name.
        /// </summary>
        public string PersonName { get; }
	}
}";

        /// <summary>
        /// The property private getter test fix code.
        /// </summary>
        private const string PropertyPrivateGetterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        public string PersonName { get; private set; }
	}
}";

        /// <summary>
        /// The property private getter test fix code.
        /// </summary>
        private const string PropertyPrivateGetterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets the person name.
        /// </summary>
        public string PersonName { get; private set; }
	}
}";

        /// <summary>
        /// The property internal getter test fix code.
        /// </summary>
        private const string PropertyInternalGetterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        public string PersonName { get; internal set; }
	}
}";

        /// <summary>
        /// The property internal getter test fix code.
        /// </summary>
        private const string PropertyInternalGetterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets the person name.
        /// </summary>
        public string PersonName { get; internal set; }
	}
}";

        /// <summary>
        /// The boolean property test code.
        /// </summary>
        private const string BooleanPropertyTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public bool IsTesterStarted { get; set; }
	}
}";

        /// <summary>
        /// The nullable datetime property test code.
        /// </summary>
        private const string NullableDateTimePropertyTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public DateTime? TestDateTime { get; set; }
	}
}";

        /// <summary>
        /// The boolean property test fix code.
        /// </summary>
        private const string BooleanPropertyTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets or Sets a value indicating whether tester is started.
        /// </summary>
        public bool IsTesterStarted { get; set; }
	}
}";

        /// <summary>
        /// The nullable boolean property test code.
        /// </summary>
        private const string NullableBooleanPropertyTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public bool? IsTesterStarted { get; set; }
	}
}";

        /// <summary>
        /// The nullable date time property test fix code.
        /// </summary>
        private const string NullableDateTimePropertyTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets or Sets the test date time.
        /// </summary>
        public DateTime? TestDateTime { get; set; }
	}
}";

        /// <summary>
        /// The nullable boolean property test fix code.
        /// </summary>
        private const string NullableBooleanPropertyTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets or Sets a value indicating whether tester is started.
        /// </summary>
        public bool? IsTesterStarted { get; set; }
	}
}";

        /// <summary>
        /// The expression body property test code.
        /// </summary>
        private const string ExpressionBodyPropertyTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
		public string PersonName => ""Person Name"";
	}
}";

        /// <summary>
        /// The expression body property test fix code.
        /// </summary>
        private const string ExpressionBodyPropertyTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class PropertyTester
	{
        /// <summary>
        /// Gets the person name.
        /// </summary>
        public string PersonName => ""Person Name"";
	}
}";


    }
    /// <summary>
    /// The property unit test.
    /// </summary>

    public partial class PropertyUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public PropertyUnitTest(TestFixure fixture)
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
        [InlineData(PropertyWithGetterSetterTestCode, PropertyWithGetterSetterTestFixCode, 10, 17)]
        [InlineData(PropertyOnlyGetterTestCode, PropertyOnlyGetterTestFixCode, 10, 17)]
        [InlineData(PropertyPrivateGetterTestCode, PropertyPrivateGetterTestFixCode, 10, 23)]
        [InlineData(PropertyInternalGetterTestCode, PropertyInternalGetterTestFixCode, 10, 23)]
        [InlineData(BooleanPropertyTestCode, BooleanPropertyTestFixCode, 10, 15)]
        [InlineData(NullableBooleanPropertyTestCode, NullableBooleanPropertyTestFixCode, 10, 16)]
        [InlineData(ExpressionBodyPropertyTestCode, ExpressionBodyPropertyTestFixCode, 10, 17)]
        [InlineData(NullableDateTimePropertyTestCode, NullableDateTimePropertyTestFixCode, 10, 20)]
        public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
        {
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

            this.VerifyCSharpDiagnostic(testCode, TestFixure.DIAG_TYPE_PUBLIC, expected);

            this.VerifyCSharpFix(testCode, fixCode, TestFixure.DIAG_TYPE_PUBLIC);
        }

        /// <summary>
        /// Gets c sharp code fix provider.
        /// </summary>
        /// <returns>A CodeFixProvider.</returns>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PropertyCodeFixProvider();
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
                return new NonPublicPropertyAnalyzer();
            }
            CodeDocumentorPackage.Options.IsEnabledForPublishMembersOnly = true;
            return new PropertyAnalyzer();
        }
    }
}
