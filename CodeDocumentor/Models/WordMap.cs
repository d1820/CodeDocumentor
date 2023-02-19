// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System;

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

        /// <summary>
        /// Gets or Sets the word evaluator.
        /// </summary>
        public Func<string, string , string> WordEvaluator { get; set; }

        /// <summary>
        /// Gets the translation.
        /// </summary>
        /// <param name="nextWord">The next word.</param>
        /// <returns>A string.</returns>
        public string GetTranslation(string nextWord)
        {
            if(WordEvaluator != null)
            {
                return WordEvaluator.Invoke(Translation, nextWord);
            }
            return Translation;
        }
    }
}
