using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Builders;
using CodeDocumentor.Services;
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
        private Mock<IOptionsService> _mockOptionsService;

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
            var mockOptionService = new Mock<IOptionsService>();
            mockOptionService.Setup(s=>s.UseNaturalLanguageForReturnNode)
                .Returns(true);
            mockOptionService.Setup(s => s.TryToIncludeCrefsForReturnTypes)
                .Returns(false);
            var comment = _builder.WithReturnType(method, mockOptionService.Object).Build();
            comment.Count.Should().Be(3);
            comment[1].ToFullString().Should().Be(@"<returns>A <typeparamref name=""TResult""/></returns>");
        }
    }
}
