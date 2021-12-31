# CodeDocumentor
---

A Visual Studio Extension to generate XML documentation automatically for c# code using IntelliSense for interface,class,enum, field, constructor, property and method.

## Excluding ErrorList Messages

To exclude analyzer messaging add the _SuppressMessage_ attribute to any member.

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

**You can see here [VS marketplace](https://marketplace.visualstudio.com/items?itemName=DanTurco.CodeDocumentor) for more information.**

Special Thanks
This was forked and modified from [jinyafeng](https://github.com/jinyafeng/DocumentationAssistant)