using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeDocumentor
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileCodeFixProvider)), Shared]
    public class FileCodeFixProvider : BaseCodeFixProvider
    {
        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }


        /// <summary>
        ///   Registers code fixes async.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <returns> A Task. </returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            await RegisterFileCodeFixesAsync(context, diagnostic);
        }
    }
}
