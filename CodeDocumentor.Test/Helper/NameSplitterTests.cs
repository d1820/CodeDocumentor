using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Common.Helpers;
using Shouldly;
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
            result.Count.ShouldBe(3);
            result[0].ShouldBe("SERVER");
        }

        [Fact]
        public void Split_ReturnsWordsSplitByUnderscore_WhenAllUppercaseStringWithNumber()
        {
            var result = NameSplitter.Split("SERVER123_ORG_CODE123");
            result.Count.ShouldBe(3);
            result[0].ShouldBe("SERVER123");
        }

        [Fact]
        public void Split_ReturnsWordsSplitByUpperCaseLetter()
        {
            var result = NameSplitter.Split("ExecuteNewActionAsync");
            result.Count.ShouldBe(4);
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionAsync");
            result.Count.ShouldBe(4);
            result.Any(a => a.Contains("OCR")).ShouldBeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingMultipleGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionFMRAsync");
            result.Count.ShouldBe(5);
            result.Any(a => a.Contains("OCR")).ShouldBeTrue();
            result.Any(a => a.Contains("FMR")).ShouldBeTrue();
        }

        //NullIntPROP

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtEnd()
        {
            var result = NameSplitter.Split("ExecuteOCRActionPROP");
            result.Count.ShouldBe(4);
            result.Any(a => a.Contains("OCR")).ShouldBeTrue();
            result.Any(a => a.Contains("PROP")).ShouldBeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtBeginning()
        {
            var result = NameSplitter.Split("PROPExecuteOCRAction");
            result.Count.ShouldBe(4);
            result.Any(a => a.Contains("OCR")).ShouldBeTrue();
            result.Any(a => a.Contains("PROP")).ShouldBeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingUnderscoresAsSpaces()
        {
            var result = NameSplitter.Split("Execute_Action");
            result.Count.ShouldBe(2);
            result.Any(a => a.Contains("Execute")).ShouldBeTrue();
            result.Any(a => a.Contains("Action")).ShouldBeTrue();
        }
    }
}
