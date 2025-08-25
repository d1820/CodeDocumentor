using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Builders;
using CodeDocumentor.Common.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Builders
{
    [SuppressMessage("XMLDocumentation", "")]
    public class DocumentationBuilderTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private DocumentationBuilder _builder;
        private Mock<ISettings> _mockSettings;

        public DocumentationBuilderTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _fixture.Initialize(output);
            _builder = new DocumentationBuilder();
        }

        [Fact]
        public void CreateReturnComment__ReturnsValidNameWithStartingWord_WhenUseNaturalLanguageForReturnNodeIsTrue()
        {
            var method = TestFixture.BuildMethodDeclarationSyntax("TResult", "Tester");
            _fixture.MockSettings.UseNaturalLanguageForReturnNode = true;
            _fixture.MockSettings.TryToIncludeCrefsForReturnTypes = false;
            var comment = _builder.WithReturnType(method, _fixture.MockSettings.UseNaturalLanguageForReturnNode, _fixture.MockSettings.TryToIncludeCrefsForReturnTypes, _fixture.MockSettings.WordMaps).Build();
            comment.Count.Should().Be(3);
            comment[1].ToFullString().Should().Be(@"<returns>A <typeparamref name=""TResult""/></returns>");
        }
    }
}
