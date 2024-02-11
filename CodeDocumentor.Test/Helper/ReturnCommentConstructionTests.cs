using System.Diagnostics.CodeAnalysis;
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
    public class ReturnCommentConstructionTests : IClassFixture<TestFixture>
    {
        private readonly ReturnCommentConstruction _returnCommentBuilder;

        public ReturnCommentConstructionTests(TestFixture testFixure, ITestOutputHelper output)
        {
            _returnCommentBuilder = new ReturnCommentConstruction();
            testFixure.Initialize(output);
            Translator.Initialize(CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>());
        }

        #region ReadOnlyCollection

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", SyntaxKind.StringKeyword);

            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A read only collection of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollectionOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A read only collection of list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollectionOfReadOnlyCollection()
        {
            var list = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A read only collection of a read only collection of strings.");
        }

        #endregion

        #region List

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromList()
        {
            var roc = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("List", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfListOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var list2 = TestFixture.BuildGenericNameSyntax("List", list);

            var roc = TestFixture.BuildGenericNameSyntax("List", list2);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of a list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIList()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIListOfIList()
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IList", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfInt()
        {
            var roc = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.IntKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of integers.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfListOfInt()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.IntKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IList", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of a list of integers.");
        }

        #endregion

        #region IEnumerable

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIEnumerable()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIEnumerableOfIEnumerable()
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IEnumerable", list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A list of a list of strings.");
        }

        #endregion

        #region ICollection

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromICollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("ICollection", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A collection of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromCollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("Collection", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A collection of strings.");
        }

        #endregion

        #region Dictionary

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIDictionary()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IDictionary", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A dictionary with a key of type string and a value of type string.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIDictionaryOfInt()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IDictionary", SyntaxKind.IntKeyword, SyntaxKind.IntKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A dictionary with a key of type integer and a value of type integer.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionary()
        {
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A dictionary with a key of type string and a value of type string.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionaryWithListValue()
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A dictionary with a key of type string and a value of type list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionaryWithListOfListValue()
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var list2 = TestFixture.BuildGenericNameSyntax("List", list);
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list2);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("A dictionary with a key of type string and a value of type list of a list of strings.");
        }

        #endregion

        #region Task & ActionResult & ValueTask

        [Theory]
        [InlineData("Task", true, "and return a")]
        [InlineData("Task", false, "returns a", true)]
        [InlineData("ValueTask", true, "and return a")]
        [InlineData("ValueTask", false, "returns a", true)]
        [InlineData("ActionResult", true, "and return an")]
        [InlineData("ActionResult", false, "returns an", true)]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfString(string type, bool buildWithAndPrefixForTaskTypes, string prefix, bool hasPeriod = false)
        {
            var roc = TestFixture.BuildGenericNameSyntax(type, SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                BuildWithAndPrefixForTaskTypes = buildWithAndPrefixForTaskTypes
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of type string" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", true, "and return a")]
        [InlineData("Task", false, "returns a", true)]
        [InlineData("ValueTask", true, "and return a")]
        [InlineData("ValueTask", false, "returns a", true)]
        [InlineData("ActionResult", true, "and return an")]
        [InlineData("ActionResult", false, "returns an", true)]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfList(string type, bool buildWithAndPrefixForTaskTypes, string prefix, bool hasPeriod = false)
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax(type, list);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                BuildWithAndPrefixForTaskTypes = buildWithAndPrefixForTaskTypes
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of a list of strings" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", true, "and return a")]
        [InlineData("Task", false, "returns a", true)]
        [InlineData("ValueTask", true, "and return a")]
        [InlineData("ValueTask", false, "returns a", true)]
        [InlineData("ActionResult", true, "and return an")]
        [InlineData("ActionResult", false, "returns an", true)]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfDictionary(string type, bool buildWithAndPrefixForTaskTypes, string prefix, bool hasPeriod = false)
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var dict = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list);
            var roc = TestFixture.BuildGenericNameSyntax(type, dict);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                BuildWithAndPrefixForTaskTypes = buildWithAndPrefixForTaskTypes
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of a dictionary with a key of type string and a value of type list of strings" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", true, "and return a")]
        [InlineData("Task", false, "returns a", true)]
        [InlineData("ValueTask", true, "and return a")]
        [InlineData("ValueTask", false, "returns a", true)]
        [InlineData("ActionResult", true, "and return an")]
        [InlineData("ActionResult", false, "returns an", true)]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfCustomDoubleGenericType(string type, bool buildWithAndPrefixForTaskTypes, string prefix, bool hasPeriod = false)
        {
            var custom = TestFixture.BuildGenericNameSyntax("CustomDoubleGenericType", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax(type, custom);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                BuildWithAndPrefixForTaskTypes = buildWithAndPrefixForTaskTypes
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of type CustomDoubleGenericType" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", true, "and return a")]
        [InlineData("Task", false, "returns a", true)]
        [InlineData("ValueTask", true, "and return a")]
        [InlineData("ValueTask", false, "returns a", true)]
        [InlineData("ActionResult", true, "and return an")]
        [InlineData("ActionResult", false, "returns an", true)]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfCustomClass(string type, bool buildWithAndPrefixForTaskTypes, string prefix, bool hasPeriod = false)
        {
            var roc = SyntaxFactory.ParseTypeName($"{type}< CustomClass>");
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                BuildWithAndPrefixForTaskTypes = buildWithAndPrefixForTaskTypes
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of type <see cref=\"CustomClass\"/>" + (hasPeriod ? "." : ""));
        }



        #endregion

        #region Unknown

        public class CustomClass<TIn, TOut>
        {
            public TIn InProp { get; set; }

            public TOut MyProperty { get; set; }
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromUnknown()
        {
            var roc = TestFixture.BuildGenericNameSyntax("Span", SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("Span");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromUnknownGeneric()
        {
            var roc = TestFixture.BuildGenericNameSyntax("CustomClass", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(roc, options);
            comment.Should().Be("CustomClass");
        }

        #endregion

        #region IdentifierNameSyntax

        [Fact]
        public void IdentifierNameSyntaxComment_CreatesValidTypeParamRef()
        {
            var roc = TestFixture.BuildMethodDeclarationSyntax("CustomClass", "TestMethod");
            var returnType = TestFixture.GetReturnType(roc);

            returnType.Should().NotBeNull();
            var options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false
            };
            var comment = _returnCommentBuilder.BuildComment(returnType, options);
            comment.Should().Be("<typeparamref name=\"CustomClass\"></typeparamref>");
        }

        #endregion
    }
}
