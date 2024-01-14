using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
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
        //[Fact(Skip ="No")]
        //public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        //{
        //    _fixture.RegisterCallback(_fixture.CurrentTestName, (o) => o.ExcludeAsyncSuffix = true);
        //    var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
        //    result.Count.Should().Be(3);

        //    var ff = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
        //    _output.WriteLine(ff.ExcludeAsyncSuffix.ToString());
        //}

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
            var ff = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            _output.WriteLine(ff.ExcludeAsyncSuffix.ToString());
        }
    }
}
