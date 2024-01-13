using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Test.TestHelpers;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class CommentHelperTests: IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public CommentHelperTests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        //SpilitNameAndToLower
        [Fact]
        [Priority(1)]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        {
            fixture.OptionsPropertyCallback = (o) => o.ExcludeAsyncSuffix = true;
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
            result.Count.Should().Be(3);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
        }

        [Fact]
        [Priority(2)]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasingAddsAsyncToListWhenOptionFalse()
        {
            fixture.OptionsPropertyCallback = (o) => o.ExcludeAsyncSuffix = false;
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
            result.Count.Should().Be(4);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
            result[3].All(a => char.IsLower(a)).Should().BeTrue();
        }
    }
}
