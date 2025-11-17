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
        int BuildComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        ClassDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, ClassDeclarationSyntax declarationSyntax);
        ClassDeclarationSyntax BuildNewDeclaration(ClassDeclarationSyntax declarationSyntax);

        // Property methods
        int BuildPropertyComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildPropertyComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        PropertyDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, PropertyDeclarationSyntax declarationSyntax);
        PropertyDeclarationSyntax BuildNewDeclaration(PropertyDeclarationSyntax declarationSyntax);

        // Constructor methods
        int BuildConstructorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildConstructorComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        ConstructorDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, ConstructorDeclarationSyntax declarationSyntax);
        ConstructorDeclarationSyntax BuildNewDeclaration(ConstructorDeclarationSyntax declarationSyntax);

        // Enum methods
        int BuildEnumComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildEnumComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        EnumDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, EnumDeclarationSyntax declarationSyntax);
        EnumDeclarationSyntax BuildNewDeclaration(EnumDeclarationSyntax declarationSyntax);

        // Field methods
        int BuildFieldComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildFieldComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        FieldDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, FieldDeclarationSyntax declarationSyntax);
        FieldDeclarationSyntax BuildNewDeclaration(FieldDeclarationSyntax declarationSyntax);

        // Method methods
        int BuildMethodComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildMethodComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        MethodDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, MethodDeclarationSyntax declarationSyntax);
        MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax);

        // Interface methods
        int BuildInterfaceComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildInterfaceComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        InterfaceDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, InterfaceDeclarationSyntax declarationSyntax);
        InterfaceDeclarationSyntax BuildNewDeclaration(InterfaceDeclarationSyntax declarationSyntax);

        // Record methods
        int BuildRecordComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        int BuildRecordComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace);
        RecordDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, RecordDeclarationSyntax declarationSyntax);
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

        /// <summary>
        /// Calculates the number of non-empty lines in the XML documentation comments that precede the specified syntax
        /// node.
        /// </summary>
        /// <remarks>Only lines within single-line or multi-line XML documentation comments are counted.
        /// Blank or whitespace-only lines are excluded from the count.</remarks>
        /// <param name="node">The syntax node whose leading XML documentation comment lines are to be counted.</param>
        /// <returns>The number of non-empty lines found in the single-line or multi-line XML documentation comments immediately
        /// preceding the specified node.</returns>
        int GetDocumentationLineCount(SyntaxNode node);
    }
}
