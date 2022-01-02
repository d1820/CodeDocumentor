using System.Collections.ObjectModel;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;

namespace CodeDocumentor.Test.Helper
{
    public class DocumentationHeaderHelperTests
    {
        [Fact]
        public void CreateReturnElementSyntax_ReturnsTypeParamRefAsEmbededNodeInReturn()
        {
            var str = "<typeparamref name=\"TResult\"></typeparamref>";
            var expected = "<returns>A <typeparamref name=\"TResult\"></typeparamref></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }

        [Fact]
        public void CreateReturnElementSyntax_ReturnsCDATAOfTaskInReturn()
        {
            var str = "Task<int>";
            var expected = "<returns><![CDATA[Task<int>]]></returns>";
            var result = DocumentationHeaderHelper.CreateReturnElementSyntax(str);
            result.ToFullString().Should().Be(expected);
        }
    }

    public class TranslatorTests
    {
        public TranslatorTests()
        {
            CodeDocumentorPackage.Options = TestFixture.BuildOptionsPageGrid();
            CodeDocumentorPackage.Options.WordMaps.Add(new WordMap { Word = "You're", Translation = "You Are" });
            CodeDocumentorPackage.Options.WordMaps.Add(new WordMap { Word = "This is long", Translation = "How long is this" });
        }

        [Theory]
        [InlineData("int", "integer")]
        [InlineData("Int32", "integer")]
        [InlineData("Int64", "integer")]
        [InlineData("OfList", "OfLists")]
        [InlineData("With OfCollection", "With OfCollections")]
        [InlineData("OfEnumerable", "OfLists")]
        [InlineData("IEnumerable", "List")]
        [InlineData("ICollection", "Collection")]
        [InlineData("IReadOnlyCollection", "Read Only Collection")]
        [InlineData("IList", "List")]
        [InlineData("IReadOnlyDictionary", "Read Only Dictionary")]
        [InlineData("IReadOnlyList", "Read Only List")]
        [InlineData("IInterfaceTester", "IInterfaceTester")]
        [InlineData("When You're the best", "When You Are the best")]
        [InlineData("Why have it.This is long.Stop", "Why have it.How long is this.Stop")]
        [InlineData("Int case check", "Int case check")]
        public void TranslateText_RrturnsTranslatedStrings(string input, string output)
        {
            
            var translated = input.Translate();
            translated.Should().Be(output);

        }
    }
}
