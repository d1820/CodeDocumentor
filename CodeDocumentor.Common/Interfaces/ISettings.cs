using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Common.Interfaces
{
    public interface ISettings : IBaseSettings
    {
        DiagnosticSeverity? ClassDiagnosticSeverity { get; set; }

        DiagnosticSeverity? ConstructorDiagnosticSeverity { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating the default severity for the code analyzer.
        /// </summary>
        /// <value> A DiagnosticSeverity. </value>
        DiagnosticSeverity DefaultDiagnosticSeverity { get; set; }

        DiagnosticSeverity? EnumDiagnosticSeverity { get; set; }

        DiagnosticSeverity? FieldDiagnosticSeverity { get; set; }

        DiagnosticSeverity? InterfaceDiagnosticSeverity { get; set; }

        DiagnosticSeverity? MethodDiagnosticSeverity { get; set; }

        DiagnosticSeverity? PropertyDiagnosticSeverity { get; set; }

        DiagnosticSeverity? RecordDiagnosticSeverity { get; set; }

        /// <summary>
        ///   Gets or Sets a value indicating whether to use the .editorconfig file for settings.
        /// </summary>
        /// <remarks> This will convert the existing settings to a %USERPROFILE% .editorconfig file </remarks>
        bool UseEditorConfigForSettings { get; set; }

        ISettings Clone();
    }
}
