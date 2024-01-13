using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Test.TestHelpers;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class CommentHelperTests: IClassFixture<TestFixture>
    {
        private readonly TestFixture fixture;

        public CommentHelperTests(TestFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.fixture.Initialize(output);
        }

        //SpilitNameAndToLower
        [Fact]
        [Priority(1)]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        {
            fixture.RegisterCallback(nameof(SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing), (o) => o.ExcludeAsyncSuffix = true);
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
            fixture.RegisterCallback(nameof(SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasingAddsAsyncToListWhenOptionFalse), (o) => o.ExcludeAsyncSuffix = false);
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync", true);
            result.Count.Should().Be(4);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
            result[3].All(a => char.IsLower(a)).Should().BeTrue();
        }
    }
}
