# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test

```
# Restore packages
nuget restore CodeDocumentor.sln

# Build (no VS deployment)
msbuild CodeDocumentor.sln /p:configuration="Release" /p:DeployExtension=false /p:ZipPackageCompressionLevel=normal

# Debug build (deploys to VS Experimental Instance)
msbuild CodeDocumentor.sln /p:configuration="Debug"

# Run tests
vstest.console.exe CodeDocumentor.Test\bin\Release\net48\CodeDocumentor.Test.dll
```

CI runs on `windows-latest` via `.github/workflows/dotnet.yml`.

## Architecture

Visual Studio extension (VSIX) that generates XML doc comments (`///`) for C# code members using Roslyn -- no network calls.

### Projects

| Project | Role |
|---|---|
| `CodeDocumentor2026` | Main VSIX (.NET 4.8, VS2022/VS2026). Entry point: `CodeDocumentor2026Package.cs` |
| `CodeDocumentor.Common` | Core logic: comment generation, Roslyn tree manipulation (netstandard2.0) |
| `CodeDocumentor.Test` | xUnit + MSTest suite (net48) |
| `CodeDocumentor.Analyzers` | Legacy Roslyn DiagnosticAnalyzer approach -- kept for reference, not used by 2026 extension |
| `CodeDocumentor` | Older VS2022-era VSIX -- retained but superseded |

### Data Flow

```
User action (context menu / editor command)
  → Command.Execute()
    → CommentExecutor          -- iterates selected DTE project items, recurses folders
      → TextSelectionExecutor  -- grabs raw C# from editor, inserts result, calls SmartFormat
        → CommentBuilderService.AddDocumentation(fileContents)
            → CSharpSyntaxTree.ParseText()
            → Build*Comments() per member type  (two passes: members first, then containers)
            → root.ReplaceNodes()
            → returns modified source string
```

### Key Files in `CodeDocumentor.Common`

- `Services/CommentBuilderService.cs` -- central service; `AddDocumentation()` is the main entry; processes Properties/Constructors/Enums/Fields/Methods first, then Interfaces/Classes/Records (second pass because they contain the first)
- `Builders/DocumentationBuilder.cs` -- fluent Roslyn `XmlNodeSyntax` builder; methods: `WithSummary`, `WithParameters`, `WithTypeParamters`, `WithReturnType`, `WithExceptionTypes`, `WithPropertyValueTypes`, `WithExisting`
- `Helper/CommentHelper.cs` -- converts PascalCase identifiers to human-readable summary text via WordMaps, pluralization, and article insertion
- `Helper/DocumentationHeaderHelper.cs` -- creates Roslyn XML element syntax nodes
- `Helpers/NameSplitter.cs` -- splits PascalCase/camelCase into word parts
- `Locators/ServiceLocator.cs` -- static singleton locator for `DocumentationHeaderHelper`, `CommentHelper`, `GenericCommentManager`, `ICommentBuilderService`, `IEventLogger`, `ISettingService`
- `Models/Settings2026.cs` -- all user settings: async suffix, value nodes, public-only, preserve existing summary, natural language returns, cref inclusion, word maps, TODO fallback

### Supported Member Types (8 total)

`CommentBuilderService` handles these Roslyn syntax node types. Each has a `Build*Comments()` method and a `BuildNewDeclaration()` overload, and is registered in `IsDocumentableNode()` and `BuildNewDocumentationNode()`:

| C# construct | Roslyn type | Batch |
|---|---|---|
| class | `ClassDeclarationSyntax` | 2 (container) |
| interface | `InterfaceDeclarationSyntax` | 2 (container) |
| record | `RecordDeclarationSyntax` | 2 (container) |
| enum | `EnumDeclarationSyntax` | 1 (member) |
| method | `MethodDeclarationSyntax` | 1 (member) |
| property | `PropertyDeclarationSyntax` | 1 (member) |
| constructor | `ConstructorDeclarationSyntax` | 1 (member) |
| field | `FieldDeclarationSyntax` | 1 (member) |

Two-pass `root.ReplaceNodes()` is intentional: batch 1 (members) runs first so container nodes in batch 2 already contain updated children.

### Adding a New Member Type

To add support for a new C# syntax node type (e.g. `EventFieldDeclarationSyntax`):

1. Add a `Build*Comments()` method in `CommentBuilderService.cs` (follow existing pattern -- collect old/new node pairs, return a replacement dictionary)
2. Add a `BuildNewDeclaration(XxxSyntax node, ...)` overload in `CommentBuilderService.cs`
3. Register in `IsDocumentableNode()` switch (line ~528)
4. Register in `BuildNewDocumentationNode()` switch (line ~566)
5. Add to the appropriate batch in `AddDocumentation()` (batch 1 for members, batch 2 for containers)
6. Add to `ICommentBuilderService` interface
7. Use `DocumentationBuilder` fluent methods -- `WithSummary` always; add `WithParameters` / `WithReturnType` / `WithExceptionTypes` as applicable
8. Use `DocumentationHeaderHelper` to create raw XML nodes; `CommentHelper` to generate human-readable text from identifier names

### Known Missing Types (not yet supported)

- `EventFieldDeclarationSyntax` -- `public event EventHandler Click;` (field-like, most common form)
- `EventDeclarationSyntax` -- explicit `add`/`remove` event accessors
- `DelegateDeclarationSyntax` -- `public delegate void MyHandler(...)`
- `StructDeclarationSyntax` -- `struct` (mirrors class; would go in batch 2)
- `IndexerDeclarationSyntax` -- `this[int index]`
- `DestructorDeclarationSyntax` -- `~MyClass()`
- `OperatorDeclarationSyntax` -- `operator+` etc.
- `ConversionOperatorDeclarationSyntax` -- `implicit`/`explicit` operator
- `EnumMemberDeclarationSyntax` -- individual enum values (whole enum is supported, members are not)

### Commands (`CodeDocumentor2026/Commands/`)

- `CodeDocumentorContextCommand` -- Solution Explorer right-click on file/folder
- `CodeDocumentorEditorCommand` -- Editor right-click "Document This" for single member
- `CodeDocumentorFileMenu` -- Tools menu "Document File"

### Settings

`OptionPageGrid` in the VSIX reads settings and populates `IBaseSettings`. `ServiceLocator.SettingService` is the runtime access point throughout `Common`.

### Test Project Notes

Tests reference both `CodeDocumentor.Analyzers` and old `CodeDocumentor` projects. New behavior tests should target `CodeDocumentor.Common` directly via `CommentBuilderService`.
