using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class CommentHelperTests
    {
        private readonly TestFixture fixture;

        public CommentHelperTests()
        {
            fixture = new TestFixture();
        }

        //SpilitNameAndToLower
        [Fact]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        {
            fixture.OptionsPropertyCallback = (o) => o.ExcludeAsyncSuffix = true;
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync".AsSpan(), true);
            result.Count.Should().Be(3);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
        }

        [Fact]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasingAddsAsyncToListWhenOptionFalse()
        {
            fixture.OptionsPropertyCallback = (o) => o.ExcludeAsyncSuffix = false;
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync".AsSpan(), true);
            result.Count.Should().Be(4);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
            result[3].All(a => char.IsLower(a)).Should().BeTrue();
        }
    }
}
