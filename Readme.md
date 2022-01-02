# CodeDocumentor
---

A Visual Studio Extension to generate XML documentation automatically for c# code using IntelliSense for interface,class,enum, field, constructor, property and method.

## Installation
---

Download and install the VSIX from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=DanTurco.CodeDocumentor)

## Table of Contents

<!-- toc -->

- [Settings](#settings)
  - [Word Translations](#word-translations)
  - [Recommended Settings](#recommended-settings)
  - [Example Options Screen](#example-options-screen)
- [Excluding ErrorList Messages](#excluding-errorlist-messages)
  - [Available DiagnosticId Codes](#available-diagnosticid-codes)
  - [Supported Members](#supported-members)
  - [Attribute](#attribute)
  - [Example](#example)
- [Usage Examples](#usage-examples)

<!-- tocstop -->

## Settings
---

| Setting | Description |
|--|--|
| Exclude async wording from comments|When documenting members skip adding asynchronously to the comment. |
| Include ```<value>``` node in property comments|When documenting properties add the value node with the return type |
| Enable comments for public members only|When documenting classes, fields, methods, and properties only add documentation headers if the item is public |
| Use natural language for return comments|When documenting members if the return type contains a generic then translate that item into natural language. The default uses CDATA nodes to show the exact return type. Example Enabled: ```<return>A List of Strings</return>``` Example Disabled: ``` <returns><![CDATA[Task<int>]]></returns>```|
| Use TODO comment when summary can not be determined|When documenting methods that can not create a valid summary insert TODO instead. Async is ignored in evaluation. Using this in conjunction with the vs2022 Task Window you can quickly find all summaries that could not be generated. |
| Word mappings for creating comments|When documenting if certain word are matched it will swap out to the translated mapping. |



### Word Translations

As part of the settings WordMaps can be defined to help control how you want text displayed. There 12 default WordMaps defined.

### Recommended Settings

These are the recommended settings that create the best output experience

![RecommenedSettings](./GifInstruction/RecomendedSettings.png)


### Example Options Screen

![ExampleSettings](./GifInstruction/Settings.png)

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

### Supported Members

- Class
- Method
- Interface
- Property
- Field
- Constructor
- Enum

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
---
<img src="./GifInstruction/quick action options.gif" />

<img src="./GifInstruction/short cut to quick add.gif" />

<img src="./GifInstruction/warning wave line.gif" />


---
Special Thanks
This was forked and modified from [jinyafeng](https://github.com/jinyafeng/DocumentationAssistant)