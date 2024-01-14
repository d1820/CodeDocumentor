using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class NameSplitterTests
    {
        [Fact]
        public void Split_ReturnsWordsSplitByUnderscore_WhenAllUppercaseString()
        {
            var result = NameSplitter.Split("SERVER_ORG_CODE");
            result.Count.Should().Be(3);
            result[0].Should().Be("SERVER");
        }

        [Fact]
        public void Split_ReturnsWordsSplitByUnderscore_WhenAllUppercaseStringWithNumber()
        {
            var result = NameSplitter.Split("SERVER123_ORG_CODE123");
            result.Count.Should().Be(3);
            result[0].Should().Be("SERVER123");
        }

        [Fact]
        public void Split_ReturnsWordsSplitByUpperCaseLetter()
        {
            var result = NameSplitter.Split("ExecuteNewActionAsync");
            result.Count.Should().Be(4);
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionAsync");
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingMultipleGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionFMRAsync");
            result.Count.Should().Be(5);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("FMR")).Should().BeTrue();
        }

        //NullIntPROP

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtEnd()
        {
            var result = NameSplitter.Split("ExecuteOCRActionPROP");
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("PROP")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtBegining()
        {
            var result = NameSplitter.Split("PROPExecuteOCRAction");
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("PROP")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingUnderscoresAsSpaces()
        {
            var result = NameSplitter.Split("Execute_Action");
            result.Count.Should().Be(2);
            result.Any(a => a.Contains("Execute")).Should().BeTrue();
            result.Any(a => a.Contains("Action")).Should().BeTrue();
        }
    }
}
