using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class DocumentationHeaderHelperTests
    {
        [Fact]
        public void CreateReturnElementSyntax_ReturnsTypeParamRefAsEmbeddedNodeInReturn()
        {
            const string str = "<typeparamref name=\"TResult\"></typeparamref>";
            const string expected = "<returns>A <typeparamref name=\"TResult\"></typeparamref></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCDATAOfTaskInReturn()
        {
            const string str = "Task<int>";
            const string expected = "<returns><![CDATA[Task<int>]]></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        #region GetExceptions
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

        [Fact]
        public void GetExceptions_ReturnsMatches()
        {
            var exceptions = DocumentationHeaderHelper.GetExceptions(MethodWithException);

            Assert.Single(exceptions.ToList());
        }

        [Fact]
        public void GetExceptions_ReturnsNoMatches_WhenNoExceptions()
        {
            var exceptions = DocumentationHeaderHelper.GetExceptions(MethodWithNoException);
            Assert.Empty(exceptions.ToList());
        }

        [Fact]
        public void GetExceptions_ReturnsDistinctMatches_WhenDuplicateExceptions()
        {
            var exceptions = DocumentationHeaderHelper.GetExceptions(MethodWithDuplicateException);
            Assert.Single(exceptions.ToList());
        }
        #endregion
    }
}
