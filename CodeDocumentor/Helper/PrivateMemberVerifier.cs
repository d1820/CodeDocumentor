using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   Verifies whether a member is private.
    /// </summary>
    public class PrivateMemberVerifier
    {
        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(ClassDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(InterfaceDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(FieldDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            // If the member is public, we still need to verify whether its parent class is a private class. Since we
            // don't want show warnings for public members within a private class.
            if (node.Parent is ClassDeclarationSyntax cds)
            {
                return IsPrivateMember(cds);
            }
            if (node.Parent is InterfaceDeclarationSyntax ids)
            {
                return IsPrivateMember(ids);
            }
            return false;
        }

        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(ConstructorDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            // If the member is public, we still need to verify whether its parent class is a private class. Since we
            // don't want show warnings for public members within a private class.
            if (node.Parent is ClassDeclarationSyntax cds)
            {
                return IsPrivateMember(cds);
            }
            if (node.Parent is InterfaceDeclarationSyntax ids)
            {
                return IsPrivateMember(ids);
            }
            return false;
        }

        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(PropertyDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            // If the member is public, we still need to verify whether its parent class is a private class. Since we
            // don't want show warnings for public members within a private class.
            if (node.Parent is ClassDeclarationSyntax cds)
            {
                return IsPrivateMember(cds);
            }
            if (node.Parent is InterfaceDeclarationSyntax ids)
            {
                return IsPrivateMember(ids);
            }
            return false;
        }

        /// <summary>
        /// Are the private member.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A bool.</returns>
        public static bool IsPrivateMember(MethodDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return true;
            }

            // If the member is public, we still need to verify whether its parent class is a private class. Since we
            // don't want show warnings for public members within a private class.
            if (node.Parent is ClassDeclarationSyntax cds)
            {
                return IsPrivateMember(cds);
            }
            if (node.Parent is InterfaceDeclarationSyntax ids)
            {
                return IsPrivateMember(ids);
            }
            return false;
        }
    }
}
