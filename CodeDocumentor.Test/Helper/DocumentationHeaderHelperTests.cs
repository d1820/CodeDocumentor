using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Helper;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class DocumentationHeaderHelperTests
    {
        [Fact]
        public void CreateReturnElementSyntax_ReturnsTypeParamRefAsEmbededNodeInReturn()
        {
            var str = "<typeparamref name=\"TResult\"></typeparamref>";
            var expected = "<returns>A <typeparamref name=\"TResult\"></typeparamref></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCDATAOfTaskInReturn()
        {
            var str = "Task<int>";
            var expected = "<returns><![CDATA[Task<int>]]></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }
    }
}
