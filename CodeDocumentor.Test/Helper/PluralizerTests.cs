using CodeDocumentor.Helper;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    public class PluralizerTests
    {
        [Theory]
        [InlineData("Do", "Does")]
        [InlineData("To", "Converts to")]
        [InlineData("Is", "Checks if is")]
        public void Pluralizer_ConvertsDoCorrectly(string word, string converted)
        {
            var result = Pluralizer.PluralizeCustom(Pluralizer.Pluralize(word));
            result.Should().Be(converted);
        }
    }
}
