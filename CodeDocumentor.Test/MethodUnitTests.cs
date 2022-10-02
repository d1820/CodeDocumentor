using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using CodeDocumentor.Vsix2022;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public partial class MethodUnitTest
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
	public class MethodTester
	{
		/// <inheritdoc/>
		public void ShowBasicMethodTester()
		{
		}
	}
}";

        /// <summary>
        /// The basic test code.
        /// </summary>
        private const string BasicTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public void ShowBasicMethodTester()
		{
		}
	}
}";

        /// <summary>
        /// The basic test fix code.
        /// </summary>
        private const string BasicTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the basic method tester.
        /// </summary>
        public void ShowBasicMethodTester()
		{
		}
	}
}";

        /// <summary>
        /// The method with parameter test code.
        /// </summary>
        private const string MethodWithParameterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public void ShowMethodWithParameterTester(string param1, int param2, bool param3)
		{
		}
	}
}";
        /// <summary>
        /// The method with parameter test fix code.
        /// </summary>
        private const string MethodWithParameterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with parameter tester.
        /// </summary>
        /// <param name=""param1"">The param1.</param>
        /// <param name=""param2"">The param2.</param>
        /// <param name=""param3"">If true, param3.</param>
        public void ShowMethodWithParameterTester(string param1, int param2, bool param3)
		{
		}
	}
}";

        /// <summary>
        /// The method with parameter test code.
        /// </summary>
        private const string MethodWithBooleanParameterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public void ShowMethodWithBooleanParameterTester(bool isRed, bool? isAssociatedWithAllProduct)
		{
		}
	}
}";
        /// <summary>
        /// The method with parameter test fix code.
        /// </summary>
        private const string MethodWithBooleanParameterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with boolean parameter tester.
        /// </summary>
        /// <param name=""isRed"">If true, is red.</param>
        /// <param name=""isAssociatedWithAllProduct"">If true, is associated with all product.</param>
        public void ShowMethodWithBooleanParameterTester(bool isRed, bool? isAssociatedWithAllProduct)
		{
		}
	}
}";

        /// <summary>
        /// The method with parameter test code.
        /// </summary>
        private const string MethodWithNullableStructParameterTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public void Show(DiagnosticResult? param1, int param2, bool param3)
		{
		}
	}
}";

        /// <summary>
        /// The method with parameter test fix code.
        /// </summary>
        private const string MethodWithNullableStructParameterTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""param1"">The param1.</param>
        /// <param name=""param2"">The param2.</param>
        /// <param name=""param3"">If true, param3.</param>
        public void Show(DiagnosticResult? param1, int param2, bool param3)
		{
		}
	}
}";

        /// <summary>
        /// The method with return test code.
        /// </summary>
        private const string MethodWithReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public MethodTester ShowMethodWithReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with return test fix code.
        /// </summary>
        private const string MethodWithReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with return tester.
        /// </summary>
        /// <returns>A MethodTester.</returns>
        public MethodTester ShowMethodWithReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with string return test code.
        /// </summary>
        private const string MethodWithStringReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public string ShowMethodWithStringReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with string return test fix code.
        /// </summary>
        private const string MethodWithStringReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with string return tester.
        /// </summary>
        /// <returns>A string.</returns>
        public string ShowMethodWithStringReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with object return test code.
        /// </summary>
        private const string MethodWithObjectReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public object ShowMethodWithObjectReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with object return test fix code.
        /// </summary>
        private const string MethodWithObjectReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with object return tester.
        /// </summary>
        /// <returns>An object.</returns>
        public object ShowMethodWithObjectReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with integer return test code.
        /// </summary>
        private const string MethodWithIntReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public int ShowMethodWithIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with integer return test fix code.
        /// </summary>
        private const string MethodWithIntReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with integer return tester.
        /// </summary>
        /// <returns>An int.</returns>
        public int ShowMethodWithIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list int return test code.
        /// </summary>
        private const string MethodWithListIntReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public List<int> ShowMethodWithListIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list int return test fix code.
        /// </summary>
        private const string MethodWithListIntReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list integer return tester.
        /// </summary>
        /// <returns><![CDATA[List<int>]]></returns>
        public List<int> ShowMethodWithListIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list of list int return test code.
        /// </summary>
        private const string MethodWithListListIntReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public List<List<int>> ShowMethodWithListListIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list of list int return test fix code.
        /// </summary>
        private const string MethodWithListListIntReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list list integer return tester.
        /// </summary>
        /// <returns><![CDATA[List<List<int>>]]></returns>
        public List<List<int>> ShowMethodWithListListIntReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list qualified name return test code.
        /// </summary>
        private const string MethodWithListQualifiedNameReturnTestCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
		public List<A.B> ShowMethodWithListQualifiedNameReturnTester()
		{
			return null;
		}
	}
}";

        /// <summary>
        /// The method with list qualified name return test fix code.
        /// </summary>
        private const string MethodWithListQualifiedNameReturnTestFixCode = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list qualified name return tester.
        /// </summary>
        /// <returns><![CDATA[List<A.B>]]></returns>
        public List<A.B> ShowMethodWithListQualifiedNameReturnTester()
		{
			return null;
		}
	}
}";

        private const string MethodWithException = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list list int return tester.
        /// </summary>
        /// <returns><![CDATA[List<List<int>>]]></returns>
        public List<List<int>> ShowMethodWithListListIntReturnTester()
		{
			throw new Exception(""test"");
		}
	}
}";
        private const string MethodWithNoException = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list list int return tester.
        /// </summary>
        /// <returns><![CDATA[List<List<int>>]]></returns>
        public List<List<int>> ShowMethodWithListListIntReturnTester()
		{
			return null;
		}
	}
}";
        private const string MethodWithDuplicateException = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
	public class MethodTester
	{
        /// <summary>
        /// Shows the method with list list int return tester.
        /// </summary>
        /// <returns><![CDATA[List<List<int>>]]></returns>
        public List<List<int>> ShowMethodWithListListIntReturnTester()
		{
			throw new Exception(""test"");
            throw new Exception(""test"");
		}
	}
}";
    }

    /// <summary>
    /// The method unit test.
    /// </summary>
    [SuppressMessage("XMLDocumentation", "")]
    public partial class MethodUnitTest : CodeFixVerifier, IClassFixture<TestFixure>
    {
        private readonly TestFixure _fixture;

        public MethodUnitTest(TestFixure fixture)
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
		[InlineData(BasicTestCode, BasicTestFixCode, 10, 15)]
		[InlineData(MethodWithParameterTestCode, MethodWithParameterTestFixCode, 10, 15)]
		[InlineData(MethodWithBooleanParameterTestCode, MethodWithBooleanParameterTestFixCode, 10, 15)]
		[InlineData(MethodWithNullableStructParameterTestCode, MethodWithNullableStructParameterTestFixCode, 10, 15)]
		[InlineData(MethodWithReturnTestCode, MethodWithReturnTestFixCode, 10, 23)]
		[InlineData(MethodWithStringReturnTestCode, MethodWithStringReturnTestFixCode, 10, 17)]
		[InlineData(MethodWithObjectReturnTestCode, MethodWithObjectReturnTestFixCode, 10, 17)]
		[InlineData(MethodWithIntReturnTestCode, MethodWithIntReturnTestFixCode, 10, 14)]
		[InlineData(MethodWithListIntReturnTestCode, MethodWithListIntReturnTestFixCode, 10, 20)]
		[InlineData(MethodWithListListIntReturnTestCode, MethodWithListListIntReturnTestFixCode, 10, 26)]
		[InlineData(MethodWithListQualifiedNameReturnTestCode, MethodWithListQualifiedNameReturnTestFixCode, 10, 20)]
		public void ShowDiagnosticAndFix(string testCode, string fixCode, int line, int column)
		{
            CodeDocumentorPackage.Options.UseNaturalLanguageForReturnNode = false;
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

			this.VerifyCSharpDiagnostic(testCode, TestFixure.DIAG_TYPE_PUBLIC_ONLY, expected);

			this.VerifyCSharpFix(testCode, fixCode, TestFixure.DIAG_TYPE_PUBLIC_ONLY);
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
                CodeDocumentorPackage.Options.IsEnabledForPublishMembersOnly = false;
                return new NonPublicMethodAnalyzer();
            }
            if (diagType == TestFixure.DIAG_TYPE_PUBLIC_ONLY)
            {
                CodeDocumentorPackage.Options.IsEnabledForPublishMembersOnly = true;
            }
            return new MethodAnalyzer();
		}



        #region GetExceptions
      

        [Fact]
        public async Task GetExceptions_ReturnsMatches()
        {
            var exceptions = MethodCodeFixProvider.GetExceptions(MethodWithException);

            Assert.Single(exceptions.ToList());
        }


      
        [Fact]
        public async Task GetExceptions_ReturnsNoMatches_WhenNoExceptions()
        {
            var exceptions = MethodCodeFixProvider.GetExceptions(MethodWithNoException);
            Assert.Empty(exceptions.ToList());
        }

       

        [Fact]
        public async Task GetExceptions_ReturnsDistinctMatches_WhenDuplicateExceptions()
        {
            var exceptions = MethodCodeFixProvider.GetExceptions(MethodWithDuplicateException);
            Assert.Single(exceptions.ToList());
        }
        #endregion
    }
}
