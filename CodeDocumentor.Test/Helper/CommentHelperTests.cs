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
        public CommentHelperTests()
        {
            TestFixture.BuildOptionsPageGrid();

            //sometimes these test fail locally cause of the clash of using this static setting. Dont run tests in parallel
            CodeDocumentorPackage.Options.ExcludeAsyncSuffix = true;
        }

        //SpilitNameAndToLower
        [Fact]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasing()
        {
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync".AsSpan(), true);
            result.Count.Should().Be(3);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
        }

        [Fact]
        public void SpilitNameAndToLower_KeepsAllUpperCaseWordsInProperCasingAddsAsyncToListWhenOptionTrue()
        {
            CodeDocumentorPackage.Options.ExcludeAsyncSuffix =false;
            var result = CommentHelper.SpilitNameAndToLower("ExecuteOCRActionAsync".AsSpan(), true);
            result.Count.Should().Be(4);
            result[0].All(a => char.IsLower(a)).Should().BeTrue();
            result[1].All(a => char.IsUpper(a)).Should().BeTrue();
            result[2].All(a => char.IsLower(a)).Should().BeTrue();
            result[3].All(a => char.IsLower(a)).Should().BeTrue();
        }
    }
}
