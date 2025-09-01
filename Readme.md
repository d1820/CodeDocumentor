# CodeDocumentor
---
![GitHub CI](https://img.shields.io/github/actions/workflow/status/d1820/CodeDocumentor/dotnet.yml)
![GitHub License](https://img.shields.io/github/license/d1820/CodeDocumentor?logo=github&logoColor=green)
![Visual Studio Marketplace Version (including pre-releases)](https://img.shields.io/visual-studio-marketplace/v/DanTurco.CodeDocumentor)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/DanTurco.CodeDocumentor)


A Visual Studio Extension to generate XML documentation automatically for c# code using IntelliSense for interface,class,enum, field, constructor, property and method. While VS2022 provides basic documentation capabilities this fills the gap in trying to populate the summary and return nodes. This also gives control over how the summaries are translated.

In the age of copilots this extension is still valuable when working on projects where sending code to the cloud is not possible. This creates the documentation locally on your machine. Nothing is ever sent to the cloud. No Internet connection is required for this to work.

## Installation
---

Download and install the VSIX from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=DanTurco.CodeDocumentor)

## Table of Contents

<!-- toc -->

- [Instruction](#instruction)
- [Known Issues](#known-issues)
- [Comment Ordering](#comment-ordering)
- [Supported Comment Refactorings](#supported-comment-refactorings)
- [Settings](#settings)
  - [Word Translations](#word-translations)
  - [Recommended Settings](#recommended-settings)
- [Also Supports](#also-supports)
  - [One Word Methods](#one-word-methods)
    - [Example](#example)
- [Excluding ErrorList Messages](#excluding-errorlist-messages)
  - [Available DiagnosticId Codes](#available-diagnosticid-codes)
  - [Supported Members](#supported-members)
  - [Attribute](#attribute)
  - [Example](#example-1)
- [Usage Examples](#usage-examples)
  - [Example Cref Support](#example-cref-support)
- [Errors and Crashes](#errors-and-crashes)
- [Using .editorconfig for settings](#using-editorconfig-for-settings)
- [Changelog](#changelog)
- [Special Thanks](#special-thanks)

<!-- tocstop -->

## Instruction
---

1. When you installed it successful to your Visual Studio. You can see the warning wave line below the members which don't have documentation on it.
2. Then you can click the bulb to see the fix option. When you click the option, the documentation will be added.
3. You can use shortcut(Alt+Enter or Ctrl+.) to quickly add the documentation. Documentation fixes can be implemented at the member, document, project, and solution levels.

## Known Issues

Microsoft is not going to make any changes to truly allow analyzers to run out of process. Even with .editorconfig support, it will not work if you want to have any user level settings collection from Visual Studio > Options.

- As of VS2022 version 17.6.x there is some bug that makes extension analyzers not able to work properly if you have *Run code analysis in separate process*

  ![Out of process](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/outOfProcess.png?raw=true)

  **Please disable this setting to allow CodeDocumentor to work correctly.**

- As of VS2022 Version 17.8.6. Out of process works but ONLY if you deselect *_Run code analysis on latest .NET_*.

   ![Out Of Process Latest](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/OutOfProessLatest.png?raw=true)

- As of VS2022 Version 17.14.13. Out of process does not work AGAIN. you need to deselect *_Run code analysis in separate process*.
  ![Out Of Process 17.14.13](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/OutOfProcess17.14.13.png?raw=true)
  **Please disable this setting to allow CodeDocumentor to work correctly.**


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

Code Documentor supports creating, updating, and recreating on a given type. There is also an avaialble fix at eny level to comment the whole file.

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
| Default Diagnostics                                 | Allows setting a new default diagnostic level for evaluation. Default is Warning. A restart of Visual Studio is required on change.                                                                                                                                                                    |
| Class Diagnostics                                   | Allows setting a new default diagnostic level for evaluation for classes. A restart of Visual Studio is required on change.                                                                                                                                                                            |
| Constructor Diagnostics                             | Allows setting a new default diagnostic level for evaluation for constructors. A restart of Visual Studio is required on change.                                                                                                                                                                       |
| Enum Diagnostics                                    | Allows setting a new default diagnostic level for evaluation for enums. A restart of Visual Studio is required on change.                                                                                                                                                                              |
| Field Diagnostics                                   | Allows setting a new default diagnostic level for evaluation for fields. A restart of Visual Studio is required on change.                                                                                                                                                                             |
| Interface Diagnostics                               | Allows setting a new default diagnostic level for evaluation for interfaces. A restart of Visual Studio is required on change.                                                                                                                                                                         |
| Method Diagnostics                                  | Allows setting a new default diagnostic level for evaluation for methods. A restart of Visual Studio is required on change.                                                                                                                                                                            |
| Property Diagnostics                                | Allows setting a new default diagnostic level for evaluation for properties. A restart of Visual Studio is required on change.                                                                                                                                                                         |
| Record Diagnostics                                  | Allows setting a new default diagnostic level for evaluation for records. A restart of Visual Studio is required on change.                                                                                                                                                                            |
| Use .editorconfig for settings options              | This will convert existing extension options to .editorconfig values stored in %USERPROFILE%. This allows CodeDocumentor to run out of process.|



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
| Default Diagnostics                                 | Warning     |



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



## Excluding ErrorList Messages
---

There are 2 ways to exclude analyzer messaging. Defauly level is set to **Warning**

1. Add the _SuppressMessage_ attribute to any member.
2. Add DiagnosticId exclusions to the editorconfig

### Available DiagnosticId Codes

- CD1600: Class
- CD1601: Constructor
- CD1602: Enum
- CD1603: Field
- CD1604: Interface
- CD1605: Method
- CD1606: Property
- CD1608: Record

### Supported Members

- Class
- Method
- Interface
- Property
- Field
- Constructor
- Enum
- Record

### Attribute

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
```

### Example

```csharp
//This will remove the analyzer messaging for entire class and all child members
[System.Diagnostics.CodeAnalysis.SuppressMessage("XMLDocumentation", "")]
public class Test<T>
{
	public string GetAsync(string name)
    {
		return name;
    }
    public TResult Tester<TResult>()
    {
		throw new ArgumentNullException(nameof(Tester));
		return default;
    }
}
```


## Usage Examples

Inline code notification

![Wavy line](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/warning%20wave%20line.gif?raw=true)

Add comments to a single type
![Single type add](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/SingleTypeAddComments.gif?raw=true)

Update comments to a single type
![Single type update](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/SingleTypeUpdateComments.gif?raw=true)

Update comments to a single type when preserving the summary setting is true
![Single type preserve comment](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/SingleTypeUpdatePreserveComments.gif?raw=true)

Update comments to a single type when preserving the summary setting is false
![Single type preserve comment disabled](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/SingleTypeUpdatePreserveCommentsDisabled.gif?raw=true)

Update the whole file at once
![Update whole file](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/UpdateWholeFile.gif?raw=true)

How fast comments can be added

![Quick add](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/short%20cut%20to%20quick%20add.gif?raw=true)

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

**Message Prefix**: "CodeDocumentor: "

![Event Log](https://github.com/d1820/CodeDocumentor/blob/main/GifInstruction/EventLog.png?raw=true)


## Using .editorconfig for settings

To convert existing settings to .editorconfig go to Tools > Options > CodeDocumentor and select **Use .editorconfig for settings options**. 
This will convert the existing Visual Studio Option Settings to editor config format and copy them to your clipboard. 
Paste this into a new or existing .editorconfig file in your solution.

**NOTE**: Even with using .editorconfig as your settings, Out of Process still can not be used, because the extension needs to support backward compatibility of using the Visual Studio Options.


## Changelog

| Date       | Change                                                                  | Version |
| ---------- | ----------------------------------------------------------------------- | ------- |
| 02/13/2024 | Rewrote document generator to builder pattern                           | 2.1.0.X |
|            | Increased code coverage for tests                                       |         |
|            | Added support for ```<see cref=""/>``` tags in summary and return nodes |         |
|            | Bug fixes                                                               |         |
| 02/1/2024  | Added support for ArgumentNullException.ThrowIf statements              | 2.0.1.1 |
| 09/01/2025 | Added support for storing settings in a solution level .editorconfig    | 3.0.0.0 |


## Special Thanks
This was forked and modified from [jinyafeng](https://github.com/jinyafeng/DocumentationAssistant)
