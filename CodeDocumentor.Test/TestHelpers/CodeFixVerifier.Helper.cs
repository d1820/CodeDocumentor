using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace CodeDocumentor.Test
{
    [SuppressMessage("XMLDocumentation", "")]
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        ///   Apply the inputted CodeAction to the inputted document. Meant to be used to apply codefixes.
        /// </summary>
        /// <param name="document"> The Document to apply the fix on </param>
        /// <param name="codeAction"> A CodeAction that will be applied to the Document. </param>
        /// <returns> A Document with the changes from the CodeAction </returns>
        private static async Task<Document> ApplyFixAsync(Document document, CodeAction codeAction)
        {
            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        /// <summary>
        ///   Compare two collections of Diagnostics,and return a list of any new diagnostics that appear only in the
        ///   second collection.
        ///   Note: Considers Diagnostics to be the same if they have the same Ids. In the case of multiple diagnostics
        ///         with the same Id in a row, this method may not necessarily return the new one.
        /// </summary>
        /// <param name="diagnostics"> The Diagnostics that existed in the code before the CodeFix was applied </param>
        /// <param name="newDiagnostics"> The Diagnostics that exist in the code after the CodeFix was applied </param>
        /// <returns> A list of Diagnostics that only surfaced in the code after the CodeFix was applied </returns>
        private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
        {
            var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
            var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

            var oldIndex = 0;
            var newIndex = 0;

            while (newIndex < newArray.Length)
            {
                if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
                {
                    ++oldIndex;
                    ++newIndex;
                }
                else
                {
                    yield return newArray[newIndex++];
                }
            }
        }

        /// <summary>
        ///   Get the existing compiler diagnostics on the inputted document.
        /// </summary>
        /// <param name="document"> The Document to run the compiler diagnostic analyzers on </param>
        /// <returns> The compiler diagnostics that were found in the code </returns>
        private static async Task<IEnumerable<Diagnostic>> GetCompilerDiagnosticsAsync(Document document)
        {
            return (await document.GetSemanticModelAsync()).GetDiagnostics();
        }

        /// <summary>
        ///   Given a document, turn it into a string based on the syntax root
        /// </summary>
        /// <param name="document"> The Document to be converted to a string </param>
        /// <returns> A string containing the syntax of the Document after formatting </returns>
        private static async Task<string> GetStringFromDocumentAsync(Document document)
        {
            var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation);
            var root = await simplifiedDoc.GetSyntaxRootAsync();
            root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
            return root.GetText().ToString();
        }
    }
}
