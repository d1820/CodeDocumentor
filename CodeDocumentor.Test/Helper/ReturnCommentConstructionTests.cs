using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Constructors;
using CodeDocumentor.Helper;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace CodeDocumentor.Test.Helper
{
    [SuppressMessage("XMLDocumentation", "")]
    public class ReturnCommentConstructionTests : IClassFixture<TestFixture>
    {
        private readonly ReturnCommentConstruction _returnCommentBuilder;
        private readonly ReturnTypeBuilderOptions _options;
        private readonly TestFixture _testFixture;

        public ReturnCommentConstructionTests(TestFixture testFixture, ITestOutputHelper output)
        {
            testFixture.Initialize(output);
            _returnCommentBuilder = new ReturnCommentConstruction(testFixture.MockOptionsService);

            _options = new ReturnTypeBuilderOptions
            {
                ReturnGenericTypeAsFullString = false,
                TryToIncludeCrefsForReturnTypes = true,
                IncludeStartingWordInText = true,
            };
            _testFixture = testFixture;
        }

        #region QualifiedNameSyntax

        [Theory]
        [InlineData(true, "a ")]
        [InlineData(false, "")]
        public void GenerateQualifiedNameComment_CreatesValidStringFromName(bool includeStartingWordInText, string startWord)
        {
            var roc = TestFixture.BuildQualifiedNameSyntax("System", "String");
            _options.IncludeStartingWordInText = includeStartingWordInText;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be(startWord + "<see cref=\"System.String\"/>");
        }

        [Theory]
        [InlineData(true, "an ")]
        [InlineData(false, "")]
        public void GenerateQualifiedNameComment_CreatesValidStringFromCustomInterface(bool includeStartingWordInText, string startWord)
        {
            var roc = TestFixture.BuildQualifiedNameSyntax("Angler", "IClass");
            _options.IncludeStartingWordInText = includeStartingWordInText;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be(startWord + "<see cref=\"Angler.IClass\"/>");
        }
        #endregion

        #region ArrayTypeSyntax

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GenerateArrayTypeComment_CreatesValidStringFromNameAndForcesAnPrefix(bool includeStartingWordInText)
        {
            var roc = TestFixture.BuildArrayTypeSyntax(SyntaxKind.StringKeyword);
            _options.IncludeStartingWordInText = includeStartingWordInText;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("an array of strings");
        }
        #endregion

        #region PredefinedTypeSyntax
        [Theory]
        [InlineData(true, "a ")]
        [InlineData(false, "")]
        public void GeneratePredefinedTypeSyntaxCommentWithCref_CreatesValidStringFromString(bool includeStartingWordInText, string startWord)
        {
            var roc = TestFixture.BuildPredefinedTypeSyntax(SyntaxKind.StringKeyword);
            _options.IncludeStartingWordInText = includeStartingWordInText;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be(startWord + "<see cref=\"string\"/>");
        }

        [Theory]
        [InlineData(true, "an ")]
        [InlineData(false, "")]
        public void GeneratePredefinedTypeSyntaxCommentWithCref_CreatesValidStringFromInt(bool includeStartingWordInText, string startWord)
        {
            var roc = TestFixture.BuildPredefinedTypeSyntax(SyntaxKind.IntKeyword);
            _options.IncludeStartingWordInText = includeStartingWordInText;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be(startWord + "<see cref=\"int\"/>");
        }
        #endregion

        #region ReadOnlyCollection

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", SyntaxKind.StringKeyword);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A read only collection of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollectionOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A read only collection of list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIReadOnlyCollectionOfReadOnlyCollection()
        {
            var list = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("IReadOnlyCollection", list);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A read only collection of a read only collection of strings.");
        }

        #endregion

        #region List

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromList()
        {
            var roc = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var roc = TestFixture.BuildGenericNameSyntax("List", list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfListOfList()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.StringKeyword);

            var list2 = TestFixture.BuildGenericNameSyntax("List", list);

            var roc = TestFixture.BuildGenericNameSyntax("List", list2);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of a list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIList()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIListOfIList()
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IList", list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of a list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfInt()
        {
            var roc = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.IntKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of integers.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromListOfListOfInt()
        {
            var list = TestFixture.BuildGenericNameSyntax("List", SyntaxKind.IntKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IList", list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of a list of integers.");
        }

        #endregion

        #region IEnumerable

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIEnumerable()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIEnumerableOfIEnumerable()
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("IEnumerable", list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A list of a list of strings.");
        }

        #endregion

        #region ICollection

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromICollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("ICollection", SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A collection of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromCollection()
        {
            var roc = TestFixture.BuildGenericNameSyntax("Collection", SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A collection of strings.");
        }

        #endregion

        #region Dictionary

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIDictionary()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IDictionary", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A dictionary with a key of type string and a value of type string.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromIDictionaryOfInt()
        {
            var roc = TestFixture.BuildGenericNameSyntax("IDictionary", SyntaxKind.IntKeyword, SyntaxKind.IntKeyword);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A dictionary with a key of type integer and a value of type integer.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionary()
        {
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A dictionary with a key of type string and a value of type string.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionaryWithListValue()
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A dictionary with a key of type string and a value of type list of strings.");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromDictionaryWithListOfListValue()
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var list2 = TestFixture.BuildGenericNameSyntax("List", list);
            var roc = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list2);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("A dictionary with a key of type string and a value of type list of a list of strings.");
        }

        #endregion

        #region GenericReturnTypeDefResult
        [Fact]
        public void GenericTypeDefResult_CreatesValidStringFromString()
        {
            var method = TestFixture.BuildMethodDeclarationSyntax("TResult", "Tester");
            _options.IncludeStartingWordInText = true;
            var comment = _returnCommentBuilder.BuildComment(method.ReturnType, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("a <typeparamref name=\"TResult\"/>");
        }

        #endregion

        #region Task & ActionResult & ValueTask

        [Theory]
        [InlineData("Task", "a")]
        [InlineData("ValueTask", "a")]
        [InlineData("ActionResult", "an")]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfString(string type, string prefix, bool hasPeriod = false)
        {
            var roc = TestFixture.BuildGenericNameSyntax(type, SyntaxKind.StringKeyword);

            _options.TryToIncludeCrefsForReturnTypes = true;
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of type <see cref=\"string\"/>" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", "a")]
        [InlineData("ValueTask", "a")]
        [InlineData("ActionResult", "an")]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfList(string type, string prefix, bool hasPeriod = false)
        {
            var list = TestFixture.BuildGenericNameSyntax("IList", SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax(type, list);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of a list of strings" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", "a")]
        [InlineData("ValueTask", "a")]
        [InlineData("ActionResult", "an")]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfDictionary(string type, string prefix, bool hasPeriod = false)
        {
            var list = TestFixture.BuildGenericNameSyntax("IEnumerable", SyntaxKind.StringKeyword);
            var dict = TestFixture.BuildGenericNameSyntax("Dictionary", SyntaxKind.StringKeyword, list);
            var roc = TestFixture.BuildGenericNameSyntax(type, dict);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of a dictionary with a key of type string and a value of type list of strings" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", "a")]
        [InlineData("ValueTask", "a")]
        [InlineData("ActionResult", "an")]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfCustomDoubleGenericType(string type, string prefix, bool hasPeriod = false)
        {
            var custom = TestFixture.BuildGenericNameSyntax("CustomDoubleGenericType", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);
            var roc = TestFixture.BuildGenericNameSyntax(type, custom);
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be($"{prefix} <see cref=\"{type}\"/> of type CustomDoubleGenericType" + (hasPeriod ? "." : ""));
        }

        [Theory]
        [InlineData("Task", "a")]
        [InlineData("ValueTask", "a")]
        [InlineData("ActionResult", "an")]
        public void GenerateGenericTypeComment_CreatesValidStringFromTaskOfCustomClass(string type, string prefix, bool hasPeriod = false)
        {
            var roc = SyntaxFactory.ParseTypeName($"{type}<CustomClass>");
            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
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

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("Span");
        }

        [Fact]
        public void GenerateGenericTypeComment_CreatesValidStringFromUnknownGeneric()
        {
            var roc = TestFixture.BuildGenericNameSyntax("CustomClass", SyntaxKind.StringKeyword, SyntaxKind.StringKeyword);

            var comment = _returnCommentBuilder.BuildComment(roc, _options, _testFixture.MockOptionsService.WordMaps);
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

            var comment = _returnCommentBuilder.BuildComment(returnType, _options, _testFixture.MockOptionsService.WordMaps);
            comment.Should().Be("a <typeparamref name=\"CustomClass\"/>");
        }

        #endregion
    }
}
