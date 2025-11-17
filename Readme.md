# CodeDocumentor
---
![GitHub CI](https://img.shields.io/github/actions/workflow/status/d1820/CodeDocumentor/dotnet.yml)
![GitHub License](https://img.shields.io/github/license/d1820/CodeDocumentor?logo=github&logoColor=green)
![Visual Studio Marketplace Version (including pre-releases)](https://img.shields.io/visual-studio-marketplace/v/DanTurco.CodeDocumentor)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/DanTurco.CodeDocumentor)


A Visual Studio Extension to generate XML documentation automatically for c# code using IntelliSense for interface,class,enum, field, constructor, property and method. While VS2022/VS2026 provides basic documentation capabilities this fills the gap in trying to populate the summary and return nodes. This also gives control over how the summaries are translated.

In the age of copilots this extension is still valuable when working on projects where sending code to the cloud is not possible. This creates the documentation locally on your machine. Nothing is ever sent to the cloud. No Internet connection is required for this to work.

**Looking for the Original Analyzer Based Version?**
Review the [CodeDocumentor Analyzer Readme.md](https://github.com/d1820/CodeDocumentor/blob/main/Readme2022.md)

## Installation
---

Download and install the CodeDocumentor2026 VSIX from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=DanTurco.CodeDocumentor2026)

**IMPORTANT!!** This extension no longer uses analyzers to perform the documentation generation. If you have any previous versions of CodeDocumentor installed please uninstall them first.

## Table of Contents

<!-- toc -->

- [Compatibility](#compatibility)
- [4 Ways to Invoke CodeDocumentor](#4-ways-to-invoke-codedocumentor)
  - [From the Tools menu](#from-the-tools-menu)
  - [From the Solution Explorer context menu on a project or solution](#from-the-solution-explorer-context-menu-on-a-project-or-solution)
  - [From the Solution Explorer context menu on a code file](#from-the-solution-explorer-context-menu-on-a-code-file)
  - [From the right click context menu in the code editor on a supported type](#from-the-right-click-context-menu-in-the-code-editor-on-a-supported-type)
- [Comment Ordering](#comment-ordering)
- [Supported Comment Refactorings](#supported-comment-refactorings)
- [Settings](#settings)
  - [Word Translations](#word-translations)
  - [Recommended Settings](#recommended-settings)
- [Also Supports](#also-supports)
  - [One Word Methods](#one-word-methods)
    - [Example](#example)
  - [Supported Members](#supported-members)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Usage Demo](#usage-demo)
  - [Example Cref Support](#example-cref-support)
- [Errors and Crashes](#errors-and-crashes)
- [Changelog](#changelog)
- [Special Thanks](#special-thanks)

<!-- tocstop -->

## Compatibility
---

The new CodeDocumentor2026 extension is compatible with Visual Studio 2022 and Visual Studio 2026. This is the new preferred version of CodeDocumentor moving forward. 
This does not use Analyzers and Fix Providers anymore so there is no longer a dependency on Roslyn Analyzers.
The XML documentation is now created in the editor foreground directly. No more cluttering the error list with messages. 
This gives the developer control on creating XML documentation when they want it.

## 4 Ways to Invoke CodeDocumentor

### From the Tools menu

![Solution Explorer Document Folder](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/ToolsMenu.png?raw=true)

### From the Solution Explorer context menu on a project or solution

![Solution Explorer Document Folder](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/SolutionFolder.png?raw=true)

### From the Solution Explorer context menu on a code file

![Solution Explorer Document File](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/SolutionFile.png?raw=true)

### From the right click context menu in the code editor on a supported type

Withing the editor for a given C# file you have 2 options from the right context menu
1. Add documentation to the given type the cursor is on. Note the cursor must be on the line of a supported XML documentation member. If the cursor is not on a supported member line you will not see the `Code Documentor This` menu item. See `Supported Members` below.
1. Add documentation to the whole file. 

![Editor Right Context Menu](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/RightContext.png?raw=true)


## Comment Ordering

Comments are structured in the following order

- Summary
- Generic Types *if applies
- Parameter Types *if applies
- Exception Types *if applies
- Property Value Types *if applies
- Remarks
- Examples
- Return Types *if applies

## Supported Comment Refactorings

Code Documentor supports creating, updating, and recreating XML documentation on a given member.

## Settings
---

To adjust these defaults go to Tools > Options > CodeDocumentor

| Setting                                             | Description                                                                                                                                                                                                                                                                                            |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Exclude async wording from comments                 | When documenting members skip adding asynchronously to the comment.                                                                                                                                                                                                                                    |
| Include ```<value>``` node in property comments     | When documenting properties add the value node with the return type                                                                                                                                                                                                                                    |
| Enable comments for public members only             | When documenting classes, fields, methods, and properties only add documentation headers if the item is public                                                                                                                                                                                         |
| Enable comments for non public fields               | When documenting fields allow adding documentation headers if the item is not public. This only applies to const and static fields.                                                                                                                                                                    |
| Use natural language for return comments            | When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example Enabled: ```<return>A List of Strings</return>``` Example Disabled: ``` <returns><![CDATA[Task<int>]]></returns>``` |
| Use TODO comment when summary can not be determined | When documenting methods that can not create a valid summary insert TODO instead. Async is ignored in evaluation. Using this in conjunction with the vs2022 Task Window you can quickly find all summaries that could not be generated.                                                                |
| Try to include return types in documentation        | When documenting methods and properties (and Use natural language for return comments is enabled) try to include <cref/> in the return element. In methods that are named 2 words or less try and generate ```<cref/>``` elements for those types in the method comment                                |
| Word mappings for creating comments                 | When documenting if certain word are matched it will swap out to the translated mapping.                                                                                                                                                                                                               |
| Preserve Existing Summary Text                      | When updating a comment or documenting the whole file if this is true; the summary text will not be regenerated. Defaults to true.                                                                                                                                                                     |


### Word Translations

As part of the settings WordMaps can be defined to help control how you want text displayed. There are already a set of default WordMaps defined.

### Recommended Settings

These are the recommended settings that create the best output experience

| Setting                                             | Description |
| --------------------------------------------------- | ----------- |
| Exclude async wording from comments                 | False       |
| Include ```<value>``` node in property comments     | False       |
| Enable comments for public members only             | False       |
| Enable comments for non public fields               | False       |
| Use natural language for return comments            | False       |
| Use TODO comment when summary can not be determined | True        |
| Try to include return types in documentation        | True        |
| Preserve Existing Summary Text                      | True        |



## Also Supports
---

- For method documenting it will scan the method code for any exceptions and automatically add them as exception nodes
- For method generic return types it uses XML CDATA so the actual generic type is displayed
- For method documenting where generics are used typeparam nodes are added.
- Whole file, project and solution comment adding

To adjust these defaults go to Tools > Options > CodeDocumentor

### One Word Methods

In an attempt to create valid summary statements when a method is only 1 word (plus Async suffix) we will read the return type of the method. If the method is a generic type an attempt will be 
made to create text representing that string. In the example below in the summary line CodeDocumentor added ```and return a <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/>```
This is leveraging the new setting **Try to include return types in documentation** to generate those ```<see cref=""/>``` elements.

#### Example

With **Try to include return types in documentation** enabled

```csharp
/// <summary>
/// Creates and return a <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/> asynchronously.
/// </summary>
/// <returns>A <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/></returns>
internal Task<ActionResult<ClientDto>> CreateAsync()
{
}
```

With **Try to include return types in documentation** disabled

```csharp
/// <summary>
/// Creates and return a task of type actionresult of type clientdto asynchronously.
/// </summary>
/// <returns>A <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/></returns>
internal Task<ActionResult<ClientDto>> CreateAsync()
{
}
```


### Supported Members

- Class
- Method
- Interface
- Property
- Field
- Constructor
- Enum
- Record

## Keyboard Shortcuts
A power developer setup involves setting up Keyboard shortcuts to invoke the CodeDocumentor in the editor.

- Open `Tools > Options`
- Search for Keyboard
- Open the Keyboard dialog if in VS2026)
- Search for CodeDocumentor
  ![Documentor keyboard Search](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/KeyboardSearch.png?raw=true)
- For the File Action, set a keyboard shortcut assignment. I use Cntrl+D Cntrl+F
  ![Documentor Keyboard Action File](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/FileKeyboard.png?raw=true)
- For the This Action, set a keyboard shortcut assignment. I use Cntrl+D Cntrl+D
  ![Documentor Keyboard Action File](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/DocumentThisKeyboard.png?raw=true)

## Usage Demo
![CodeDocumentor 2026 Demo](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/2026/CodeDocumentor2026Demo.gif?raw=true)


### Example Cref Support

```csharp
/// <summary>
/// Creates and return a <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/> asynchronously.
/// </summary>
/// <param name="clientDto">The client data transfer object.</param>
/// <exception cref="ArgumentException"></exception>
/// <returns>A <see cref="Task"/> of type <see cref="ActionResult"/> of type <see cref="ClientDto"/></returns>
internal Task<ActionResult<ClientDto>> CreateAsync(CreateClientDto clientDto)
{
throw new ArgumentException("test");
}
```

## Errors and Crashes

If you are finding the code documentor is crashing or causing errors.
All errors are written to the EventLog in windows. Check there for causes, and use this information to file a bug.

**Source**: "Visual Studio"

**Message Prefix**: "CodeDocumentor2026: "

![Event Log](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/EventLog.png?raw=true)



## Changelog

| Date       | Change                                                                  | Version |
| ---------- | ----------------------------------------------------------------------- | ------- |
| 11/17/2025 | New version created. Non-Analyzer version.                              | 1.0.0.0 |


## Special Thanks
This was forked and modified from [jinyafeng](https://github.com/jinyafeng/DocumentationAssistant)
