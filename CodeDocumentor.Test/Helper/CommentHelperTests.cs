using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class CommentHelperTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public CommentHelperTests(TestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _fixture.Initialize(output);
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());
        }


        [Theory]
        //[InlineData("_checkIfOneIsThere", "Check if one is there.")]
        [InlineData("_isValid", "Checks if is valid.")]
        public void CreateFieldComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateFieldComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("MarkItemDone", "int", "Marks item done.")]
        public void CreateMethodComment_ReturnsValidName(string name, string returnType, string expected)
        {
            var comment = CommentHelper.CreateMethodComment(name, SyntaxFactory.ParseTypeName(returnType));
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("IMarkItemDone", "Mark item done interface.")]
        [InlineData("IPublisher", "The publishers interface.")]
        [InlineData("INotifier", "The notifiers interface.")]
        public void CreateInterfaceComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateInterfaceComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("NominationBuilder", "The nominations builder.")]
        [InlineData("AwaitingAgentEmailHandler", "Awaiting agent email handler.")]
        [InlineData("PublishAgentEmailHandler", "Publish agent email handler.")]
        [InlineData("LoadReportEmailHandler", "Load report email handler.")]
        public void CreateClassComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateClassComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("NominationBuilder", "The nominations builder.")]
        public void CreateRecordComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateRecordComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("NominationType", "The nomination types.")]
        [InlineData("BuildAccess", "Build accesses.")]
        [InlineData("ClientRole", "The clients roles.")]
        public void CreateEnumComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateEnumComment(name);
            comment.Should().Be(expected);
        }
    }
}
