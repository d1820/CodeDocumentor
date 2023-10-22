using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary> The return comment construction. </summary>
    public class ReturnCommentConstruction : BaseReturnTypeCommentConstruction
    {
        /// <summary> Gets or Sets the array comment template. </summary>
        /// <value> A string. </value>
        public override string ArrayCommentTemplate { get; set; } = "An array of {0}";

        /// <summary> Gets or Sets the dictionary comment template. </summary>
        /// <value> A string. </value>
        public override string DictionaryCommentTemplate { get; set; } = "A dictionary with a key of type {0} and a value of type {1}";

        /// <summary> Gets or Sets the list comment template. </summary>
        /// <value> A string. </value>
        public override string ListCommentTemplate { get; set; } = "A list of {0}";

        /// <summary> Gets or Sets the read only collection comment template. </summary>
        /// <value> A string. </value>
        public override string ReadOnlyCollectionCommentTemplate { get; set; } = "A read only collection of {0}";

        public ReturnCommentConstruction() : base(true, true)
        {
        }

        /// <summary> Initializes a new instance of the <see cref="ReturnCommentConstruction" /> class. </summary>
        /// <param name="returnType"> The return type. </param>
        public ReturnCommentConstruction(TypeSyntax returnType) : base(true, true)
        {
            var optionsService = CodeDocumentorPackage.DIContainer().GetInstance<IOptionsService>();
            Comment = BuildComment(returnType, !optionsService.UseNaturalLanguageForReturnNode);
        }

        /// <summary> Builds a string comment for a summary node </summary>
        /// <param name="returnType"> </param>
        /// <param name="returnGenericTypeAsFullString">
        ///     Flag indicating if the full type should just be returned as a string
        /// </param>
        /// <returns> The comment </returns>
        internal override string BuildComment(TypeSyntax returnType, bool returnGenericTypeAsFullString)
        {
            if (returnType is PredefinedTypeSyntax)
            {
                return GeneratePredefinedTypeComment(returnType as PredefinedTypeSyntax);
            }
            return base.BuildComment(returnType, returnGenericTypeAsFullString);
        }
    }
}
