using System;
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
        public void Split_ReturnsWordsSplitByUpperCaseLetter()
        {
            var result = NameSplitter.Split("ExecuteNewActionAsync".AsSpan());
            result.Count.Should().Be(4);
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionAsync".AsSpan());
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingMultipleGroupsOfUppercaseLetters()
        {
            var result = NameSplitter.Split("ExecuteOCRActionFMRAsync".AsSpan());
            result.Count.Should().Be(5);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("FMR")).Should().BeTrue();
        }

        //NullIntPROP

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtEnd()
        {
            var result = NameSplitter.Split("ExecuteOCRActionPROP".AsSpan());
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("PROP")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingGroupsOfUppercaseLettersAtBegining()
        {
            var result = NameSplitter.Split("PROPExecuteOCRAction".AsSpan());
            result.Count.Should().Be(4);
            result.Any(a => a.Contains("OCR")).Should().BeTrue();
            result.Any(a => a.Contains("PROP")).Should().BeTrue();
        }

        [Fact]
        public void Split_ReturnsWordsHandlingUnderscoresAsSpaces()
        {
            var result = NameSplitter.Split("Execute_Action".AsSpan());
            result.Count.Should().Be(2);
            result.Any(a => a.Contains("Execute")).Should().BeTrue();
            result.Any(a => a.Contains("Action")).Should().BeTrue();
        }
    }
}
