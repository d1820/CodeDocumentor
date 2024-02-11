using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CodeDocumentor.Helper;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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
        [InlineData("_checkIfOneIsThere", "Check if one is there.")]
        [InlineData("_isValid", "Is valid.")]
        [InlineData("ConnectionString", "The connection string.")]
        [InlineData("_hasErrors", "Has errors.")]
        public void CreateFieldComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateFieldComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("MarkItemDone", "int", "Mark item done.")]
        [InlineData("DoWorkWithParams", "bool", "Does work with params.")] //with more then 2 words should be no "the"
        [InlineData("DoWorkWithParams", "int", "Does work with params.")] //with more then 2 words should be no "the"
        [InlineData("DoWork", "bool", "Does work.")]
        [InlineData("DoWork", "int", "Does the work.")]
        [InlineData("DoWorkAsync", "bool", "Does work asynchronously.", "Task")]
        [InlineData("DoWorkAsync", "int", "Does the work asynchronously.", "Task")]
        [InlineData("Check_On_The_User_OCR", "int", "Check on the user OCR.")]
        [InlineData("HasTimeout", "int", "Has the timeout.")]
        [InlineData("HasTimeout", "bool", "Has timeout.")]
        [InlineData("OnExecuteAsync", "int", "On execute asynchronously.")]
        [InlineData("ToUpperCase", "int", "Converts to upper case.")]
        [InlineData("Work", "int", "TODO: Add Summary.")]
        [InlineData("IsWork", "int", "Checks if is the work.")]
        [InlineData("IsWork", "bool", "Checks if is work.")]
        [InlineData("ToActionItem", "int", "Converts to action item.")]
        [InlineData("EnsureWork", "int", "Checks if is the work.")]
        [InlineData("EnsureWork", "bool", "Checks if is work.")]
        [InlineData("EnsureExecutesQuick", "int", "Checks if executes quick.")]
        [InlineData("Execute", "int", "Execute and return a <see cref=\"Task\"/> of type integer.", "Task")]
        [InlineData("Execute", "int", "Execute and return an <see cref=\"ActionResult\"/> of type integer.", "ActionResult")]
        [InlineData("Execute", "int", "Execute and return a <see cref=\"ValueTask\"/> of type integer.", "ValueTask")]
        [InlineData("Execute", "Person", "Execute and return a <see cref=\"ValueTask\"/> of type <see cref=\"Person\"/>.", "ValueTask")]
        [InlineData("ExecuteAsync", "string", "Execute and return a <see cref=\"ValueTask\"/> of type string.", "ValueTask", true, false)]
        [InlineData("ExecuteAsync", "string", "Execute and return a <see cref=\"ValueTask\"/> of type string asynchronously.", "ValueTask", false, false)]
        public void CreateMethodComment_ReturnsValidName(string name, string returnType, string expected, string genericReturnType = null, bool excludeAsyncSuffix = false, bool useToDoCommentsOnSummaryError = true)
        {
            _fixture.RegisterCallback(_fixture.CurrentTestName, (o) =>
            {
                o.ExcludeAsyncSuffix = excludeAsyncSuffix;
                o.UseToDoCommentsOnSummaryError = useToDoCommentsOnSummaryError;
            });
            _fixture.Initialize(_output);
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());

            TypeSyntax typeSyntax;
            if (!string.IsNullOrEmpty(genericReturnType))
            {
                typeSyntax = SyntaxFactory.ParseTypeName($"{genericReturnType}<{returnType}>");
            }
            else
            {
                typeSyntax = SyntaxFactory.ParseTypeName(returnType);
            }

            var comment = CommentHelper.CreateMethodComment(name, typeSyntax);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("IMarkItemDone", "Mark item done interface.")]
        [InlineData("IPublisher", "The publisher interface.")]
        [InlineData("INotifier", "The notifier interface.")]
        public void CreateInterfaceComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateInterfaceComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("NominationBuilder", "The nomination builder.")]
        [InlineData("AwaitingAgentEmailHandler", "The awaiting agent email handler.")]
        [InlineData("PublishAgentEmailHandler", "The publish agent email handler.")]
        [InlineData("LoadReportEmailHandler", "The load report email handler.")]
        [InlineData("ClientDto", "The client data transfer object.")]
        public void CreateClassComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateClassComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("NominationBuilder", "The nomination builder.")]
        [InlineData("ClientDto", "The client data transfer object.")]
        public void CreateRecordComment_ReturnsValidName(string name, string expected)
        {
            var comment = CommentHelper.CreateRecordComment(name);
            comment.Should().Be(expected);
        }

        [Theory]
        [InlineData("TestProperty", "Gets or sets the test property.", false, true)]
        [InlineData("IsValid", "Gets or sets a value indicating whether valid.", true, true)]
        [InlineData("HasError", "Gets a value indicating whether has error.", true, false)]
        public void CreatePropertyComment_ReturnsValidName(string name, string expected, bool isBool, bool hasSetter)
        {
            var comment = CommentHelper.CreatePropertyComment(name, isBool, hasSetter);
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

        [Theory]
        [InlineData("nominationType", "int", "The nomination type.")]
        [InlineData("buildAccess", "string", "The build access.")]
        [InlineData("hasClientRole", "bool", "If true, has client role.")]
        [InlineData("errorMessage", "string", "The error message.", true)]
        public void CreateParameterComment_ReturnsValidName(string name, string paramType, string expected, bool nullable = false)
        {
            var attributeLists = new SyntaxList<AttributeListSyntax>();
            var modifiers = new SyntaxTokenList();
            TypeSyntax typeSyntax;
            if (nullable)
            {
                var underlyingType = SyntaxFactory.ParseTypeName(paramType);
                typeSyntax = SyntaxFactory.NullableType(underlyingType);
            }
            else
            {
                typeSyntax = SyntaxFactory.ParseTypeName(paramType);
            }

            var parameter = SyntaxFactory.Parameter(attributeLists, modifiers, typeSyntax, SyntaxFactory.Identifier(name), null);
            var comment = CommentHelper.CreateParameterComment(parameter);
            comment.Should().Be(expected);
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
            var result = CommentHelper.TranslateParts(new List<string> { word });
            result.Should().Contain(converted);
        }
    }
}
