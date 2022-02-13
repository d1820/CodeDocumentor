using System;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///   The documentation header helper.
    /// </summary>
    public static class DocumentationHeaderHelper
    {
        private static Regex typeParamRegex = new Regex(@"""\w+""");

        /// <summary>
        ///   The category of the diagnostic.
        /// </summary>
        public const string Category = "CodeDocumentor";

        /// <summary>
        ///   The category to check for when excluding analyzer actions
        /// </summary>
        public const string ExclusionCategory = "XMLDocumentation";

        /// <summary>
        ///   The summary.
        /// </summary>
        public const string Summary = "summary";

        /// <summary>
        ///   The inherit doc.
        /// </summary>
        public const string InheritDoc = "inheritdoc";

        /// <summary>
        /// Are the read only collection.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsReadOnlyCollection(this GenericNameSyntax nameSyntax)
        {
            return nameSyntax.Identifier.ValueText.Contains("ReadOnlyCollection");
        }

        /// <summary>
        /// Are the read only collection.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsReadOnlyCollection(this TypeSyntax nameSyntax)
        {
            return nameSyntax.ToString().Contains("ReadOnlyCollection");
        }

        /// <summary>
        /// Are the list.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsList(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");

        }
        /// <summary>
        /// Are the list.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsList(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        /// <summary>
        /// Are the dictionary.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsDictionary(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Dictionary");
        }

        /// <summary>
        /// Are the task.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        /// <summary>
        ///   Creates only summary documentation comment trivia.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A DocumentationCommentTriviaSyntax. </returns>
        public static DocumentationCommentTriviaSyntax CreateOnlySummaryDocumentationCommentTrivia(string content)
        {
            SyntaxList<XmlNodeSyntax> list = SyntaxFactory.List(CreateSummaryPartNodes(content));
            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

        /// <summary>
        ///   Checks if a node is attributed with <see cref="System.Diagnostics.CodeAnalysis.SuppressMessage" /> with a
        ///   category of "XMLDocumentation"
        /// </summary>
        /// <param name="node"> </param>
        /// <returns> bool </returns>
        public static bool HasAnalyzerExclusion(MemberDeclarationSyntax node, bool recursive = true)
        {
            if (node == null)
            {
                return false;
            }

            var attrs = node.AttributeLists.SelectMany(w => w.Attributes);
            var hasExclusion = attrs.Where(w => w.ArgumentList != null).SelectMany(w => w.ArgumentList.Arguments).Any(w => w.Expression.IsKind(SyntaxKind.StringLiteralExpression) && w.Expression.ToString().Contains(ExclusionCategory));
            if (!hasExclusion && recursive)
            {
                return HasAnalyzerExclusion(node.Parent as MemberDeclarationSyntax, recursive);
            }
            return hasExclusion;
        }

        /// <summary>
        ///   Creates summary part nodes.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <returns> An array of XmlNodeSyntaxes. </returns>
        public static XmlNodeSyntax[] CreateSummaryPartNodes(string content)
        {
            /*
                 ///[0] <summary>
                 /// The code fix provider.
                 /// </summary>[1] [2]
             */

            // [0] " " + leading comment exterior trivia
            XmlTextSyntax xmlText0 = CreateLineStartTextSyntax();

            // [1] Summary
            XmlElementSyntax xmlElement = CreateSummaryElementSyntax(content);

            // [2] new line
            XmlTextSyntax xmlText1 = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { xmlText0, xmlElement, xmlText1 };
        }

        /// <summary>
        ///   Creates parameter part nodes.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterContent"> The parameter content. </param>
        /// <returns> An array of XmlNodeSyntaxes. </returns>
        public static XmlNodeSyntax[] CreateParameterPartNodes(string parameterName, string parameterContent)
        {
            ///[0] <param name="parameterName"></param>[1][2]

            // [0] -- line start text
            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            // [1] -- parameter text
            XmlElementSyntax parameterText = CreateParameterElementSyntax(parameterName, parameterContent);

            // [2] -- line end text
            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, parameterText, lineEndText };
        }

        /// <summary>
        ///   Creates the type parameter part nodes.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> An array of XmlNodeSyntaxes. </returns>
        public static XmlNodeSyntax[] CreateTypeParameterPartNodes(string parameterName)
        {
            ///[0] <param name="parameterName"></param>[1][2]

            // [0] -- line start text
            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            // [1] -- parameter text
            XmlElementSyntax parameterText = CreateTypeParameterElementSyntax(parameterName);

            // [2] -- line end text
            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, parameterText, lineEndText };
        }

        /// <summary>
        ///   Creates the exception nodes.
        /// </summary>
        /// <param name="exceptionType"> The exception type. </param>
        /// <returns> An array of XmlNodeSyntaxes. </returns>
        public static XmlNodeSyntax[] CreateExceptionNodes(string exceptionType)
        {
            // <exception cref = "parameterName" >

            // [0] -- line start text
            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            var identity = SyntaxFactory.IdentifierName(exceptionType.Replace("throw new", string.Empty).Trim());
            CrefSyntax cref = SyntaxFactory.NameMemberCref(identity);
            var exceptionNode = SyntaxFactory.XmlExceptionElement(cref);

            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, exceptionNode, lineEndText };
        }

        /// <summary>
        ///   Creates return part nodes.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <returns> An array of XmlNodeSyntaxes. </returns>
        public static XmlNodeSyntax[] CreateReturnPartNodes(string content)
        {
            ///[0] <returns></returns>[1][2]

            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            var returnElement = CreateReturnElementSyntax(content);

            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, returnElement, lineEndText };
        }

        /// <summary>
        /// Creates the value part nodes.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>An array of XmlNodeSyntaxes</returns>
        public static XmlNodeSyntax[] CreateValuePartNodes(string content)
        {
            ///[0] <value></value>[1][2]

            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            var returnElement = CreateReturnElementSyntax(content, "value");

            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, returnElement, lineEndText };
        }

        /// <summary>
        ///   Creates summary element syntax.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateSummaryElementSyntax(string content)
        {
            XmlNameSyntax xmlName = SyntaxFactory.XmlName(SyntaxFactory.Identifier(DocumentationHeaderHelper.Summary));
            XmlElementStartTagSyntax summaryStartTag = SyntaxFactory.XmlElementStartTag(xmlName);
            XmlElementEndTagSyntax summaryEndTag = SyntaxFactory.XmlElementEndTag(xmlName);

            return SyntaxFactory.XmlElement(
                summaryStartTag,
                SyntaxFactory.SingletonList<XmlNodeSyntax>(CreateSummaryTextSyntax(content)),
                summaryEndTag);
        }

        /// <summary>
        ///   Creates parameter element syntax.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterContent"> The parameter content. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateParameterElementSyntax(string parameterName, string parameterContent)
        {
            XmlNameSyntax paramName = SyntaxFactory.XmlName("param");

            /// <param name="parameterName"> [0][1] </param>
            /// [2]

            // [0] -- param start tag with attribute
            XmlNameAttributeSyntax paramAttribute = SyntaxFactory.XmlNameAttribute(parameterName);
            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(paramAttribute));

            // [1] -- content
            XmlTextSyntax content = SyntaxFactory.XmlText(parameterContent);

            // [2] -- end tag
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<SyntaxNode>(content), endTag);
        }

        /// <summary>
        ///   Creates the type parameter element syntax.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateTypeParameterElementSyntax(string parameterName)
        {
            XmlNameSyntax paramName = SyntaxFactory.XmlName("typeparam");

            /// <typeparam name="parameterName"> [0][1] </param> [2]

            // [0] -- param start tag with attribute
            XmlNameAttributeSyntax paramAttribute = SyntaxFactory.XmlNameAttribute(parameterName);
            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(paramAttribute));

            // [2] -- end tag
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, endTag);
        }

        /// <summary>
        ///   Creates the type parameter ref element syntax.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateTypeParameterRefElementSyntax(string parameterName)
        {
            XmlNameSyntax paramName = SyntaxFactory.XmlName("typeparamref");

            /// <typeparamref name="parameterName"> [0][1] </param> [2]

            // [0] -- param start tag with attribute
            XmlNameAttributeSyntax paramAttribute = SyntaxFactory.XmlNameAttribute(parameterName);
            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(paramAttribute));

            // [2] -- end tag
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, endTag);
        }

        /// <summary>
        ///   Creates return element syntax.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <param name="xmlNodeName"> The xml node type to create </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlNodeSyntax CreateReturnElementSyntax(string content, string xmlNodeName = "returns")
        {
            XmlNameSyntax xmlName = SyntaxFactory.XmlName(xmlNodeName);
            /// <returns> [0]xxx[1] </returns>
            /// [2]

            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(xmlName);
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(xmlName);

            var cleanContent = content?.Trim();
            var xmlParseResponse = IsXML(cleanContent);

            if (xmlParseResponse.isTypeParam)
            {
                try
                {
                    var text = SyntaxFactory.XmlText($"A ");
                    var name = typeParamRegex.Match(cleanContent).Value.Replace("\"", string.Empty);
                    var typeParamNode = CreateTypeParameterRefElementSyntax(name);

                    var list = SyntaxFactory.SingletonList<XmlNodeSyntax>(text);
                    list = list.Add(typeParamNode);
                    return SyntaxFactory.XmlElement(startTag, list, endTag);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex.ToString());
                    //If we fail then create empty return
                    var text = SyntaxFactory.XmlText("");
                    var list = SyntaxFactory.SingletonList<XmlNodeSyntax>(text);
                    return SyntaxFactory.XmlElement(startTag, list, endTag);
                }
            }
            if (xmlParseResponse.isGeneric || xmlParseResponse.isXml)
            {
                //Wrap any xml thats not a typeParamRef to CDATA
                SyntaxToken text1Token = SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(), cleanContent, cleanContent, SyntaxFactory.TriviaList());
                var tokens = SyntaxFactory.TokenList().Add(text1Token);
                var cdata = SyntaxFactory.XmlCDataSection(SyntaxFactory.Token(SyntaxKind.XmlCDataStartToken), tokens, SyntaxFactory.Token(SyntaxKind.XmlCDataEndToken));

                return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(cdata), endTag);
            }

            XmlTextSyntax contentText = SyntaxFactory.XmlText(cleanContent);
            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(contentText), endTag);
        }

        /// <summary>
        ///   Checks if string is XML or GenericType
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> Tuple of bool and bool </returns>
        internal static (bool isXml, bool isGeneric, bool isTypeParam) IsXML(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (false, false, false);
            }
            var isXml = Regex.IsMatch(text, @"(\<\w+\s)");
            var isGeneric = Regex.IsMatch(text, @"(\w+\<)");
            var isTypeParam = Regex.IsMatch(text, @"(<typeparam)");

            return (isXml, isGeneric, isTypeParam);
        }

        /// <summary>
        ///   Creates summary text syntax.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A XmlTextSyntax. </returns>
        internal static XmlTextSyntax CreateSummaryTextSyntax(string content)
        {
            content = " " + content;
            /*
                /// <summary>[0]
                /// The code fix provider.[1] [2]
                ///[3] </summary>
             */

            // [0] -- NewLine token
            SyntaxToken newLine0Token = CreateNewLineToken();

            // [1] -- Content + leading comment exterior trivia
            SyntaxTriviaList leadingTrivia = CreateCommentExterior();
            SyntaxToken text1Token = SyntaxFactory.XmlTextLiteral(leadingTrivia, content, content, SyntaxFactory.TriviaList());

            // [2] -- NewLine token
            SyntaxToken newLine2Token = CreateNewLineToken();

            // [3] -- " " + leading comment exterior
            SyntaxTriviaList leadingTrivia2 = CreateCommentExterior();
            SyntaxToken text2Token = SyntaxFactory.XmlTextLiteral(leadingTrivia2, " ", " ", SyntaxFactory.TriviaList());

            return SyntaxFactory.XmlText(newLine0Token, text1Token, newLine2Token, text2Token);
        }

        /// <summary>
        ///   Creates line start text syntax.
        /// </summary>
        /// <returns> A XmlTextSyntax. </returns>
        private static XmlTextSyntax CreateLineStartTextSyntax()
        {
            /*
                ///[0] <summary>
                /// The code fix provider.
                /// </summary>
            */

            // [0] " " + leading comment exterior trivia
            SyntaxTriviaList xmlText0Leading = CreateCommentExterior();
            SyntaxToken xmlText0LiteralToken = SyntaxFactory.XmlTextLiteral(xmlText0Leading, " ", " ", SyntaxFactory.TriviaList());
            XmlTextSyntax xmlText0 = SyntaxFactory.XmlText(xmlText0LiteralToken);
            return xmlText0;
        }

        /// <summary>
        ///   Creates line end text syntax.
        /// </summary>
        /// <returns> A XmlTextSyntax. </returns>
        private static XmlTextSyntax CreateLineEndTextSyntax()
        {
            /*
                /// <summary>
                ///   The code fix provider.
                /// </summary>
                /// [0]
            */

            // [0] end line token.
            SyntaxToken xmlTextNewLineToken = CreateNewLineToken();
            XmlTextSyntax xmlText = SyntaxFactory.XmlText(xmlTextNewLineToken);
            return xmlText;
        }

        /// <summary>
        ///   Creates new line token.
        /// </summary>
        /// <returns> A SyntaxToken. </returns>
        private static SyntaxToken CreateNewLineToken()
        {
            return SyntaxFactory.XmlTextNewLine(Environment.NewLine, false);
        }

        /// <summary>
        ///   Creates comment exterior.
        /// </summary>
        /// <returns> A SyntaxTriviaList. </returns>
        private static SyntaxTriviaList CreateCommentExterior()
        {
            return SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("///"));
        }
    }
}
