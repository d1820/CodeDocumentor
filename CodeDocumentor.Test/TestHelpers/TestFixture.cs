using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public static class TestFixture
    {
        public static IOptionPageGrid BuildOptionsPageGrid()
        {
            CodeDocumentorPackage.Options = new TestOptionsPageGrid();

            //FieldInfo field = typeof(CodeDocumentorPackage).GetField("_options", BindingFlags.NonPublic | BindingFlags.Static);
            //var opts = (TestOptionsPageGrid)field.GetValue(null);
            //if (opts == null)
            //{
            //    CodeDocumentorPackage.Options = options;
            //}
            return CodeDocumentorPackage.Options;
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

        public static IdentifierNameSyntax GetReturnType(this MethodDeclarationSyntax methodDeclaration)
        {
            foreach (var childNode in methodDeclaration.ChildNodes())
            {
                if (childNode.IsKind(SyntaxKind.IdentifierName)){
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
