// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public static class Runtime
    {
        /// <summary>
        /// Gets or Sets a value indicating whether running unit tests.
        /// </summary>
        /// <value>A bool.</value>
        public static bool RunningUnitTests { get; set; }
    }
}
