using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeDocumentor.Analyzers;
using CodeDocumentor.Analyzers.Locators;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Services;
using CodeDocumentor.Test.TestHelpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

[assembly: TestCaseOrderer(PriorityOrderer.FullName, PriorityOrderer.AssemblyName)]
[assembly: SuppressMessage("XMLDocumentation", "")]
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace CodeDocumentor.Test
{
    public static class TestFixtureExtensions
    {
        public static string GetTestName(this ITestOutputHelper output, bool returnFullName = true)
        {
            var type = output.GetType();
            var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            var test = (ITest)testMember.GetValue(output);
            if (returnFullName)
            {
                return test.DisplayName;
            }
            var name = test.DisplayName.Split('(').First();
            return returnFullName ? name : name.Split('.').Last();
        }
    }

    [SuppressMessage("XMLDocumentation", "")]
    public class TestFixture
    {
        public const string DIAG_TYPE_PUBLIC = "public";
        public const string DIAG_TYPE_PUBLIC_ONLY = "publicOnly";
        public const string DIAG_TYPE_PRIVATE = "private";

        public string CurrentTestName { get; set; }

        public TestSettings MockSettings;
        protected static ConcurrentDictionary<string, Action<ISettings>> RegisteredCallBacks = new ConcurrentDictionary<string, Action<ISettings>>();

        public TestFixture()
        {
            Runtime.RunningUnitTests = true;
        }

        public void Initialize(ITestOutputHelper output)
        {
            CurrentTestName = output.GetTestName();

            MockSettings = new TestSettings();
            BaseCodeFixProvider.SetSettings(MockSettings);
            BaseDiagnosticAnalyzer.SetSettings(MockSettings);
            ServiceLocator.SettingService = new SettingService();
            ServiceLocator.Logger = new Logger();
        }

        public void SetPublicProcessingOption(ISettings o, string diagType)
        {
            if (diagType == DIAG_TYPE_PRIVATE)
            {
                o.IsEnabledForPublicMembersOnly = false;
            }
            if (diagType == DIAG_TYPE_PUBLIC_ONLY)
            {
                o.IsEnabledForPublicMembersOnly = true;
            }
        }

        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }

        public void AssertOutputContainsCount(string[] source, string searchTerm, int numOfTimes)
        {
            var matchQuery = from word in source
                             where word.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1
                             select word;

            matchQuery.Count().Should().Be(numOfTimes);
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, SyntaxKind innerKindKey, SyntaxKind innerKindValue)
        {
            var keyNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindKey));
            var valueNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindValue));
            var nodes = new List<TypeSyntax> { keyNode, valueNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, SyntaxKind innerKindKey, GenericNameSyntax innerNode)
        {
            var keyNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindKey));
            var nodes = new List<TypeSyntax> { keyNode, innerNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static QualifiedNameSyntax BuildQualifiedNameSyntax(string part1, string part2)
        {
            // Create the left identifier
            var leftIdentifier = SyntaxFactory.IdentifierName(part1);

            // Create the right identifier
            var rightIdentifier = SyntaxFactory.IdentifierName(part2);

            // Create the qualified name syntax
            return SyntaxFactory.QualifiedName(leftIdentifier, rightIdentifier);

        }

        public static PredefinedTypeSyntax BuildPredefinedTypeSyntax(SyntaxKind kind)
        {
            return SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));
        }

        public static ArrayTypeSyntax BuildArrayTypeSyntax(SyntaxKind kind)
        {
            // Create the element type (e.g., int)
            var elementType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));

            // Create the array type syntax with a single dimension
            return SyntaxFactory.ArrayType(elementType, SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier()));
        }


        public static GenericNameSyntax BuildGenericNameSyntax(string listType, SyntaxKind innerKind)
        {
            var stringNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKind));
            var nodes = new List<TypeSyntax> { stringNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, GenericNameSyntax innerNode)
        {
            var nodes = new List<TypeSyntax> { innerNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static MethodDeclarationSyntax BuildMethodDeclarationSyntax(string typeName, string methodName)
        {
            var seperatedSyntaxList = SyntaxFactory.List<AttributeListSyntax>();

            var access = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
            var accessTokenList = SyntaxFactory.TokenList(access);

            var returnType = SyntaxFactory.IdentifierName(typeName);
            var method = SyntaxFactory.Identifier(methodName);
            var genericType = SyntaxFactory.TypeParameter(typeName);

            var nodes = new List<TypeParameterSyntax> { genericType };
            var tplSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var tpl = SyntaxFactory.TypeParameterList(tplSyntaxList);
            var parameterList = SyntaxFactory.ParameterList();
            var constraints = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();

            var body = SyntaxFactory.Block();
            var semi = SyntaxFactory.Token(SyntaxKind.SemicolonToken);
            var item = SyntaxFactory.MethodDeclaration(seperatedSyntaxList, accessTokenList, returnType, null, method, tpl, parameterList, constraints, body, semi);
            return item;
        }

        public static IdentifierNameSyntax GetReturnType(MethodDeclarationSyntax methodDeclaration)
        {
            foreach (var childNode in methodDeclaration.ChildNodes())
            {
                if (childNode.IsKind(SyntaxKind.IdentifierName))
                {
                    return childNode as IdentifierNameSyntax;
                }
            }
            return null;
        }

        public static IdentifierNameSyntax BuildIdentifierNameSyntax(string typeName)
        {
            var item = SyntaxFactory.IdentifierName(typeName);
            return item;
        }
    }
}
