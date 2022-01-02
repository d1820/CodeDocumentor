// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    public class WordMap
    {
        /// <summary>
        /// Gets or Sets the word.
        /// </summary>
        /// <value>A string.</value>
        public string Word { get; set; }

        /// <summary>
        /// Gets or Sets the translation.
        /// </summary>
        /// <value>A string.</value>
        public string Translation { get; set; }
    }
}
