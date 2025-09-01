using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace CodeDocumentor.Helper
{
    public static class WordExtensions
    {
        public static bool IsBoolReturnType(this TypeSyntax returnType)
        {
            return returnType.ToString().IndexOf("bool", StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
