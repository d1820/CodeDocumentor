using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeDocumentor.Builders;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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

        public DocumentationBuilderTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _fixture.Initialize(output);
            _builder = new DocumentationBuilder();
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());
        }

        [Fact]
        public void CreateReturnComment__ReturnsValidNameWithStartingWord_WhenUseNaturalLanguageForReturnNodeIsTrue()
        {
            var method = TestFixture.BuildMethodDeclarationSyntax("TResult", "Tester");
            var comment = _builder.WithReturnType(method).Build();
            comment.Count.Should().Be(3);
            comment[1].ToFullString().Should().Be(@"<returns>A <typeparamref name=""TResult""/></returns>");
        }
    }
}
