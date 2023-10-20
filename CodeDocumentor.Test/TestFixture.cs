using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using SimpleInjector;
using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Test.TestHelpers;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public class TestFixture
    {
        public const string DIAG_TYPE_PUBLIC = "public";
        public const string DIAG_TYPE_PUBLIC_ONLY = "publicOnly";
        public const string DIAG_TYPE_PRIVATE = "private";

        public Action<IOptionsService> OptionsPropertyCallback { get; set; }

        public TestFixture()
        {
            Runtime.RunningUnitTests = true;

            var testContainer = new Container();
            testContainer.Register<IOptionsService>(() =>
            {
                var os = new TestOptionsService();
                OptionsPropertyCallback?.Invoke(os);
                return os;
            }, Lifestyle.Transient);
            CodeDocumentorPackage.DIContainer(testContainer);
        }

        public void SetPublicProcessingOption(IOptionsService o, string diagType)
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
            SyntaxNode keyNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindKey));
            SyntaxNode valueNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindValue));
            var nodes = new List<SyntaxNode> { keyNode, valueNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, SyntaxKind innerKindKey, GenericNameSyntax innerNode)
        {
            SyntaxNode keyNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKindKey));
            var nodes = new List<SyntaxNode> { keyNode, innerNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, SyntaxKind innerKind)
        {
            SyntaxNode stringNode = SyntaxFactory.PredefinedType(SyntaxFactory.Token(innerKind));
            var nodes = new List<SyntaxNode> { stringNode };
            var seperatedSyntaxList = SyntaxFactory.SeparatedList(nodes);
            var args = SyntaxFactory.TypeArgumentList(seperatedSyntaxList);
            var identifier = SyntaxFactory.Identifier(listType);
            var item = SyntaxFactory.GenericName(identifier, args);
            return item;
        }

        public static GenericNameSyntax BuildGenericNameSyntax(string listType, GenericNameSyntax innerNode)
        {
            var nodes = new List<SyntaxNode> { innerNode };
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
