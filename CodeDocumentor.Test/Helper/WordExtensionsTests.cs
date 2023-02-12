using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    public class WordExtensionsTests
    {
        [Theory]
        [InlineData("Do", "Work", true)]
        [InlineData("To", "Uppercase", true)]
        public void IsVerbCombo_HandlesWordCorrectly(string first, string second, bool result)
        {
            var isVerb = first.IsVerbCombo(second);
            isVerb.Should().Be(result);
        }
    }
}
