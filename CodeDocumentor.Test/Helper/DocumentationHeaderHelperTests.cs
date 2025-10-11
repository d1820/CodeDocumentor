using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Analyzers.Helper;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class DocumentationHeaderHelperTests : IClassFixture<TestFixture>
    {
        public DocumentationHeaderHelperTests(TestFixture fixture, ITestOutputHelper output)
        {
            fixture.Initialize(output);
            _documentationHeaderHelper = new DocumentationHeaderHelper();
        }
        [Fact]
        public void CreateReturnElementSyntax_ReturnsMultipleCRefAsEmbeddedNodeInReturn()
        {
            const string str = "A <see cref=\"Task\"/> of type <typeparamref name=\"TResult\"/>";
            const string expected = "<returns>A <see cref=\"Task\"/> of type <typeparamref name=\"TResult\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsTypeParamRefAsEmbeddedNodeInReturn()
        {
            const string str = "A <typeparamref name=\"TResult\"/>";
            const string expected = "<returns>A <typeparamref name=\"TResult\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCDATAOfTaskInReturn()
        {
            const string str = "Task<int>";
            const string expected = "<returns><![CDATA[Task<int>]]></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCDATAOfCDATATaskInReturn()
        {
            const string str = "<![CDATA[Task<int>]]>";
            const string expected = "<returns><![CDATA[Task<int>]]></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCRefOfTypeInReturn()
        {
            const string str = "<see cref=\"MasterClass\"/>";
            const string expected = "<returns><see cref=\"MasterClass\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }
        [Fact]
        public void CreateReturnElementSyntax_ReturnsStringAndCRefOfTypeInReturn()
        {
            const string str = "Returns a <see cref=\"MasterClass\"/>";
            const string expected = "<returns>Returns a <see cref=\"MasterClass\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsStringAndCRefOfInterfaceTypeInReturn()
        {
            const string str = "Returns an <see cref=\"IMasterClass\"/>";
            const string expected = "<returns>Returns an <see cref=\"IMasterClass\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsStringAnd2CRefOfTypesInReturn()
        {
            const string str = "Returns a <see cref=\"Task\"/> of type <see cref=\"MasterClass\"/>";
            const string expected = "<returns>Returns a <see cref=\"Task\"/> of type <see cref=\"MasterClass\"/></returns>";
            var result = _documentationHeaderHelper.CreateReturnElementSyntax(str);
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

        private const string MethodWithExceptionAndThrowIfHelperException = @"
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
            ArgumentNullException.ThrowIfNull("");
		}
	}
}";
        private DocumentationHeaderHelper _documentationHeaderHelper;

        [Fact]
        public void GetExceptions_ReturnsMatches()
        {
            var exceptions = _documentationHeaderHelper.GetExceptions(MethodWithException);

            Assert.Single(exceptions.ToList());
        }

        [Fact]
        public void GetExceptions_ReturnsNoMatches_WhenNoExceptions()
        {
            var exceptions = _documentationHeaderHelper.GetExceptions(MethodWithNoException);
            Assert.Empty(exceptions.ToList());
        }

        [Fact]
        public void GetExceptions_ReturnsDistinctMatches_WhenDuplicateExceptions()
        {
            var exceptions = _documentationHeaderHelper.GetExceptions(MethodWithDuplicateException);
            Assert.Single(exceptions.ToList());
        }

        [Fact]
        public void GetExceptions_ReturnsTwoMatches_WhenExceptionAndThrowIfHelperException()
        {
            var exceptions = _documentationHeaderHelper.GetExceptions(MethodWithExceptionAndThrowIfHelperException);
            Assert.Equal(2, exceptions.ToList().Count);
        }
        #endregion
    }
}
