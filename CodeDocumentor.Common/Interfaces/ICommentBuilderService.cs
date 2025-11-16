using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Common.Interfaces
{
    public interface ICommentBuilderService
    {
        // Class methods (existing)
        int BuildComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        ClassDeclarationSyntax BuildNewDeclaration(ISettings settings, ClassDeclarationSyntax declarationSyntax);
        ClassDeclarationSyntax BuildNewDeclaration(ClassDeclarationSyntax declarationSyntax);

        // Property methods
        int BuildPropertyComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildPropertyComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        PropertyDeclarationSyntax BuildNewDeclaration(ISettings settings, PropertyDeclarationSyntax declarationSyntax);
        PropertyDeclarationSyntax BuildNewDeclaration(PropertyDeclarationSyntax declarationSyntax);

        // Constructor methods
        int BuildConstructorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildConstructorComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        ConstructorDeclarationSyntax BuildNewDeclaration(ISettings settings, ConstructorDeclarationSyntax declarationSyntax);
        ConstructorDeclarationSyntax BuildNewDeclaration(ConstructorDeclarationSyntax declarationSyntax);

        // Enum methods
        int BuildEnumComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildEnumComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        EnumDeclarationSyntax BuildNewDeclaration(ISettings settings, EnumDeclarationSyntax declarationSyntax);
        EnumDeclarationSyntax BuildNewDeclaration(EnumDeclarationSyntax declarationSyntax);

        // Field methods
        int BuildFieldComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildFieldComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        FieldDeclarationSyntax BuildNewDeclaration(ISettings settings, FieldDeclarationSyntax declarationSyntax);
        FieldDeclarationSyntax BuildNewDeclaration(FieldDeclarationSyntax declarationSyntax);

        // Method methods
        int BuildMethodComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildMethodComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        MethodDeclarationSyntax BuildNewDeclaration(ISettings settings, MethodDeclarationSyntax declarationSyntax);
        MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax);

        // Interface methods
        int BuildInterfaceComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildInterfaceComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        InterfaceDeclarationSyntax BuildNewDeclaration(ISettings settings, InterfaceDeclarationSyntax declarationSyntax);
        InterfaceDeclarationSyntax BuildNewDeclaration(InterfaceDeclarationSyntax declarationSyntax);

        // Record methods
        int BuildRecordComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildRecordComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        RecordDeclarationSyntax BuildNewDeclaration(ISettings settings, RecordDeclarationSyntax declarationSyntax);
        RecordDeclarationSyntax BuildNewDeclaration(RecordDeclarationSyntax declarationSyntax);

        // Utility methods
        string AddDocumentation(string fileContents);

        /// <summary>
        /// Determines if a syntax node is documentable (can have XML documentation comments)
        /// </summary>
        bool IsDocumentableNode(SyntaxNode node);

        /// <summary>
        /// Builds documentation for any supported syntax node type
        /// </summary>
        SyntaxNode BuildNewDocumentationNode(SyntaxNode node);
    }
}
