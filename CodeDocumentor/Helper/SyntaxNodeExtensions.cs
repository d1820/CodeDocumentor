using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public static class SyntaxNodeExtensions
    {
        public static bool IsOwnedByInterface(this SyntaxNode node) {
            return node?.Parent.GetType() == typeof(InterfaceDeclarationSyntax);
        }
    }
}
