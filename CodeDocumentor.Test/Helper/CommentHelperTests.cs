using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class CommentHelperTests: IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public CommentHelperTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _fixture.Initialize(output);
        }

        //SpilitNameAndToLower
        [Fact]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        {
            _fixture.RegisterCallback(_fixture.CurrentTestName, (o) => o.ExcludeAsyncSuffix = true);
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
            result.Count.Should().Be(3);
            result[0].Should().Be("execute");
        }

        [Theory]
        [InlineData(false, 4)]
        [InlineData(true, 3)]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasingAddsAsyncToListWhenOptionFalse(bool exclude, int expectedCount)
        {
            _fixture.RegisterCallback(_fixture.CurrentTestName, (o) => o.ExcludeAsyncSuffix = exclude);
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
            result.Count.Should().Be(expectedCount);
            if (expectedCount == 4)
            {
                result[0].All(a => char.IsLower(a)).Should().BeTrue();
                result[1].All(a => char.IsUpper(a)).Should().BeTrue();
                result[2].All(a => char.IsLower(a)).Should().BeTrue();
                result[3].All(a => char.IsLower(a)).Should().BeTrue();
            }else if(expectedCount == 3)
            {
                result[0].All(a => char.IsLower(a)).Should().BeTrue();
                result[1].All(a => char.IsUpper(a)).Should().BeTrue();
                result[2].All(a => char.IsLower(a)).Should().BeTrue();
            }
        }

        [Fact]
        public void CreateFieldComment_ReturnsValidName()
        {
            var comment = CommentHelper.CreateFieldComment("_checkIfOneIsThere");
            comment.Should().Be("_checkIfOneIsThere");
        }

        [Fact]
        public void CreateMethodComment_ReturnsValidName()
        {
            var comment = CommentHelper.CreateMethodComment("MarkItemDone", SyntaxFactory.ParseTypeName("int"));
            comment.Should().Be("_checkIfOneIsThere");
        }

        [Fact]
        public void CreateInterfaceComment_ReturnsValidName()
        {
            var comment = CommentHelper.CreateInterfaceComment("IMarkItemDone");
            comment.Should().Be("_checkIfOneIsThere");
        }
    }
}
