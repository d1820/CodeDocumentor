using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class TranslatorTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixure;
        private readonly ITestOutputHelper _output;

        public TranslatorTests(TestFixture fixture, ITestOutputHelper output)
        {
            _testFixure = fixture;
            _output = output;
            fixture.Initialize(output);
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
        [InlineData("Do Work", "Does Work")]
        [InlineData("To UpperCase", "Converts to UpperCase")]
        public void TranslateText_ReturnsTranslatedStrings(string input, string output)
        {
            _testFixure.RegisterCallback(_testFixure.CurrentTestName, (o) => {
                var temp = o.WordMaps.ToList();
                temp.Add(new WordMap { Word = "You're", Translation = "You Are" });
                temp.Add(new WordMap { Word = "This is long", Translation = "How long is this" });
                o.WordMaps = temp.ToArray();
            });
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());
            var translated = input.ApplyUserTranslations();
            translated.Should().Be(output);
        }

        [Theory]
        [InlineData("Do", "Does")]
        [InlineData("To", "Converts to")]
        [InlineData("Is", "Checks if is")]
        [InlineData("Ensure", "Checks if is")]
        [InlineData("Dto", "Data transfer object")]
        public void InteralTranslate_ConvertsCorrectly(string word, string converted)
        {
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());
            var result = Translator.InternalTranslateText(word, 99);
            result.Should().Be(converted);
        }
    }
}
