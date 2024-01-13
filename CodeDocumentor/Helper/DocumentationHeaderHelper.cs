using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CodeDocumentor.Vsix2022;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleInjector.Diagnostics;

namespace CodeDocumentor.Helper
{
    /// <summary> The documentation header helper. </summary>
    public static class DocumentationHeaderHelper
    {
        /// <summary> The category of the diagnostic. </summary>
        public const string CATEGORY = "CodeDocumentor";

        /// <summary> The example. </summary>
        public const string EXAMPLE = "example";

        /// <summary> The category to check for when excluding analyzer actions </summary>
        public const string EXCLUSION_CATEGORY = "XMLDocumentation";

        /// <summary> The inherit doc. </summary>
        public const string INHERITDOC = "inheritdoc";

        /// <summary> The remarks. </summary>
        public const string REMARKS = "remarks";

        /// <summary> The summary. </summary>
        public const string SUMMARY = "summary";

        /// <summary> Creates the exception nodes. </summary>
        /// <param name="exceptionType"> The exception type. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
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

        /// <summary> Creates the only summary documentation comment trivia. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A DocumentationCommentTriviaSyntax. </returns>
        public static DocumentationCommentTriviaSyntax CreateOnlySummaryDocumentationCommentTrivia(string content)
        {
            SyntaxList<XmlNodeSyntax> list = SyntaxFactory.List(CreateSummaryPartNodes(content));
            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

        /// <summary> Creates the parameter part nodes. </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterContent"> The parameter content. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
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

        /// <summary> Has analyzer exclusion. </summary>
        /// <param name="node"> The node. </param>
        /// <param name="recursive"> If true, recursive. </param>
        /// <returns> A bool. </returns>
        public static bool HasAnalyzerExclusion(SyntaxNode node, bool recursive = true, List<AttributeSyntax> attrs = null)
        {
            if (node == null)
            {
                return false;
            }
            if (attrs == null)
            {
                attrs = new List<AttributeSyntax>();
            }

            if (node is MemberDeclarationSyntax memSyntax)
            {
                attrs.AddRange(GetAttributes(memSyntax));
            }

            if (node is CompilationUnitSyntax compSyntax)
            {
                attrs.AddRange(GetAttributes(compSyntax));
            }

            var hasExclusion = attrs.Any();
            if (!hasExclusion && recursive)
            {
                return HasAnalyzerExclusion(node.Parent, recursive, attrs);
            }
            return hasExclusion;
        }

        private static IEnumerable<AttributeSyntax> GetAttributes(CompilationUnitSyntax node)
        {
            if (node == null)
            {
                return new SyntaxList<AttributeSyntax>();
            }

            var attrs = node.AttributeLists.SelectMany(w => w.Attributes);
            return attrs.Where(w => w.ArgumentList != null)
                         .SelectMany(w => w.ArgumentList.Arguments
                                .Where(ss => ss.Expression.IsKind(SyntaxKind.StringLiteralExpression) && ss.Expression.ToString().Contains(EXCLUSION_CATEGORY))
                                .Select(ss => w));

        }

        private static IEnumerable<AttributeSyntax> GetAttributes(MemberDeclarationSyntax node)
        {
            if (node == null)
            {
                return new SyntaxList<AttributeSyntax>();
            }

            var attrs = node.AttributeLists.SelectMany(w => w.Attributes);
            return attrs.Where(w => w.ArgumentList != null)
                         .SelectMany(w => w.ArgumentList.Arguments
                                .Where(ss => ss.Expression.IsKind(SyntaxKind.StringLiteralExpression) && ss.Expression.ToString().Contains(EXCLUSION_CATEGORY))
                                .Select(ss => w));

        }

        /// <summary> Checks if is dictionary. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsDictionary(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Dictionary");
        }

        /// <summary> Checks if is list. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        /// <summary> Checks if is list. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        public static bool IsPropertyReturnTypeBool(this PropertyDeclarationSyntax declarationSyntax)
        {
            bool isBoolean = false;
            if (declarationSyntax.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = ((PredefinedTypeSyntax)declarationSyntax.Type).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (declarationSyntax.Type.IsKind(SyntaxKind.NullableType))
            {
                var returnType = ((NullableTypeSyntax)declarationSyntax.Type).ElementType as PredefinedTypeSyntax;
                if (returnType != null)
                {
                    isBoolean = returnType.ToString().IndexOf("bool", StringComparison.OrdinalIgnoreCase) > -1;
                }
            }

            return isBoolean;
        }

        /// <summary> Checks if is read only collection. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this GenericNameSyntax nameSyntax)
        {
            return nameSyntax.Identifier.ValueText.Contains("ReadOnlyCollection");
        }

        /// <summary> Checks if is read only collection. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this TypeSyntax nameSyntax)
        {
            return nameSyntax.ToString().Contains("ReadOnlyCollection");
        }

        /// <summary> Checks if is task. </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        /// <summary>
        /// Checks if is generic action result.
        /// </summary>
        /// <param name="nameSyntax">The name syntax.</param>
        /// <returns>A bool.</returns>
        public static bool IsGenericActionResult(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("ActionResult", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        public static bool PropertyHasSetter(this PropertyDeclarationSyntax declarationSyntax)
        {
            bool hasSetter = false;

            if (declarationSyntax.AccessorList != null && declarationSyntax.AccessorList.Accessors.Any(o => o.Kind() == SyntaxKind.SetAccessorDeclaration))
            {
                if (declarationSyntax.AccessorList.Accessors.First(o => o.Kind() == SyntaxKind.SetAccessorDeclaration).ChildTokens().Any(o => o.IsKind(SyntaxKind.PrivateKeyword) || o.IsKind(SyntaxKind.InternalKeyword)))
                {
                    // private set or internal set should consider as no set.
                    hasSetter = false;
                }
                else
                {
                    hasSetter = true;
                }
            }

            return hasSetter;
        }

        /// <summary> Creates the parameter element syntax. </summary>
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
            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(content), endTag);
        }

        /// <summary>
        /// Determines started word.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <param name="useProperCasing">If true, use proper casing.</param>
        /// <returns>A string.</returns>
        internal static string DetermineStartingWord(ReadOnlySpan<char> returnType, bool useProperCasing = true)
        {
            if (returnType.IsEmpty)
            {
                return string.Empty;
            }
            var str = returnType.ToString();
            //if the returnType alread starts with a or an then just return
            if (str.StartsWith("a ", StringComparison.InvariantCultureIgnoreCase) || str.StartsWith("an ", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }
            var vowelChars = new List<char>() { 'a', 'e', 'i', 'o', 'u' };
            if (vowelChars.Contains(char.ToLower(returnType[0])))
            {
                return useProperCasing ? "An" : "an";
            }
            else
            {
                return useProperCasing ? "A" : "a";
            }
        }


        /// <summary> Create the return element syntax. </summary>
        /// <param name="content"> The content. </param>
        /// <param name="xmlNodeName"> The xml node name. </param>
        /// <returns> A XmlNodeSyntax. </returns>
        internal static XmlNodeSyntax CreateReturnElementSyntax(string content, string xmlNodeName = "returns")
        {
            XmlNameSyntax xmlName = SyntaxFactory.XmlName(xmlNodeName);
            /// <returns> [0]xxx[1] </returns>
            /// [2]

            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(xmlName);
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(xmlName);

            var regex = $@"<{xmlNodeName}>(.+)<\/{xmlNodeName}>";

            var cleanContent = (content ?? string.Empty).Trim();
            var plueckedReturnElemement = Regex.Match(cleanContent, regex);
            //if we get in a full <returns>fff</returns> we pull the inner text from the node and create the corret XmlNodes
            if (plueckedReturnElemement == null || plueckedReturnElemement.Groups.Count == 0)
            {
                XmlTextSyntax contentText = SyntaxFactory.XmlText(cleanContent);
                return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(contentText), endTag);
            }
            cleanContent = plueckedReturnElemement.Success ? plueckedReturnElemement.Groups[0].Value : cleanContent;

            var xmlParseResponse = new XmlInformation(cleanContent);
            if (xmlParseResponse.IsTypeParam)
            {
                try
                {
                    //var startingWord = DetermineStartingWord(cleanContent.AsSpan(), true);
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
            if (xmlParseResponse.IsGeneric || xmlParseResponse.IsXml)
            {
                //Wrap any xml thats not a typeParamRef to CDATA
                SyntaxToken text1Token = SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(), cleanContent, cleanContent, SyntaxFactory.TriviaList());
                var tokens = SyntaxFactory.TokenList().Add(text1Token);
                var cdata = SyntaxFactory.XmlCDataSection(SyntaxFactory.Token(SyntaxKind.XmlCDataStartToken), tokens, SyntaxFactory.Token(SyntaxKind.XmlCDataEndToken));

                return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(cdata), endTag);
            }

            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(cleanContent)), endTag);
        }

        /// <summary> Creates the summary element syntax. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateSummaryElementSyntax(string content)
        {
            XmlNameSyntax xmlName = SyntaxFactory.XmlName(SyntaxFactory.Identifier(DocumentationHeaderHelper.SUMMARY));
            XmlElementStartTagSyntax summaryStartTag = SyntaxFactory.XmlElementStartTag(xmlName);
            XmlElementEndTagSyntax summaryEndTag = SyntaxFactory.XmlElementEndTag(xmlName);

            return SyntaxFactory.XmlElement(
                summaryStartTag,
                SyntaxFactory.SingletonList<XmlNodeSyntax>(CreateSummaryTextSyntax(content)),
                summaryEndTag);
        }

        /// <summary> Creates the summary text syntax. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> A XmlTextSyntax. </returns>
        internal static XmlTextSyntax CreateSummaryTextSyntax(string content)
        {
            content = " " + content ?? string.Empty;
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

        /// <summary> Create the type parameter element syntax. </summary>
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

        /// <summary> Create the type parameter ref element syntax. </summary>
        /// <param name="parameterName"> The parameter name. <see cref="XmlElementSyntax"/> </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax CreateTypeParameterRefElementSyntax(string parameterName)
        {
            XmlNameSyntax paramName = SyntaxFactory.XmlName("typeparamref");

            /// <typeparamref name="parameterName"> [0][1] </typeparamref> [2]

            // [0] -- param start tag with attribute
            XmlNameAttributeSyntax paramAttribute = SyntaxFactory.XmlNameAttribute(parameterName);
            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(paramAttribute));

            // [2] -- end tag
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, endTag);
        }

        /// <summary>
        /// Creates the see C ref element syntax.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns>A <see cref="XmlElementSyntax"/>.</returns>
        internal static XmlElementSyntax CreateSeeCRefElementSyntax(string typeName)
        {
            XmlNameSyntax paramName = SyntaxFactory.XmlName("see");

            /// <see cref="typeName" />

            // [0] -- param start tag with attribute
            XmlTextAttributeSyntax creftribute = SyntaxFactory.XmlTextAttribute("cref", typeName);
            XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(creftribute));

            // [2] -- end tag
            XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, endTag);

        }

        /// <summary> Gets the element syntax. </summary>
        /// <param name="syntax"> The syntax. </param>
        /// <param name="name"> The name. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal static XmlElementSyntax GetElementSyntax(this CSharpSyntaxNode syntax, string name)
        {
            if (syntax.HasLeadingTrivia)
            {
                var docComment = syntax.GetLeadingTrivia().FirstOrDefault(a => a.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                                                                || a.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                                                || a.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia));
                if (docComment != default)
                {
                    var docTriviaSyntax = docComment.GetStructure() as DocumentationCommentTriviaSyntax;
                    var items = docTriviaSyntax?.Content
                        .OfType<XmlElementSyntax>();

                    var match = items
                        .FirstOrDefault(element => string.Equals(element.StartTag.Name.LocalName.ValueText, name, StringComparison.OrdinalIgnoreCase));

                    return match;

                    //if (summaryElement != null)
                    //{
                    //    // Get the text inside the <summary> element
                    //    string summaryText = string.Join("", summaryElement.ChildNodes()
                    //        .OfType<XmlTextSyntax>()
                    //        .Select(node => node.TextTokens.ToFullString()));
                    //    var parts = summaryText.Trim().Split(new[] { "///" }, StringSplitOptions.RemoveEmptyEntries).Select(s=> "/// " + s);
                    //    return string.Join(Environment.NewLine, parts);
                    //}
                }
            }
            return null;
        }

        /// <summary> Has summary. </summary>
        /// <param name="syntax"> The syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool HasSummary(this CSharpSyntaxNode syntax)
        {
            return syntax.HasLeadingTrivia && syntax.GetLeadingTrivia().Any(a => a.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia));
        }

        /// <summary> Checks if is private. </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool IsPrivate(this BaseMethodDeclarationSyntax declarationSyntax)
        {
            bool isPrivate = false;
            if (declarationSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                isPrivate = true;
            }
            return isPrivate;
        }

        /// <summary> Upserts the leading trivia. </summary>
        /// <param name="leadingTrivia"> The leading trivia. </param>
        /// <param name="commentTrivia"> The comment trivia. </param>
        /// <returns> A SyntaxTriviaList. </returns>
        internal static SyntaxTriviaList UpsertLeadingTrivia(this SyntaxTriviaList leadingTrivia, DocumentationCommentTriviaSyntax commentTrivia)
        {
            if (leadingTrivia.All(a => a.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                return leadingTrivia.Add(SyntaxFactory.Trivia(commentTrivia));
            }

            var existingIndex = leadingTrivia.Select((node, index) => new { node, index }).FirstOrDefault(
                        f =>
                         f.node.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                            || f.node.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                            || f.node.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                        )?.index ?? -1;
            if (existingIndex < 0)
            {
                return leadingTrivia.Insert(leadingTrivia.Count - 1, SyntaxFactory.Trivia(commentTrivia));
            }
            else
            {
                return leadingTrivia.Replace(leadingTrivia[existingIndex], SyntaxFactory.Trivia(commentTrivia));
            }
        }

        #region Builders

        /// <summary> Gets the exceptions. </summary>
        /// <param name="textToSearch"> The text to search. </param>
        /// <returns> <![CDATA[IEnumerable<string>]]> </returns>
        internal static IEnumerable<string> GetExceptions(string textToSearch)
        {
            if (string.IsNullOrEmpty(textToSearch))
            {
                return Enumerable.Empty<string>();
            }
            var exceptions = regEx.Matches(textToSearch).OfType<Match>()
                                                        .Select(m => m?.Groups[0]?.Value)
                                                        .Distinct();
            return exceptions;
        }

        /// <summary> Withs the exception types. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithExceptionTypes(this SyntaxList<XmlNodeSyntax> list, MethodDeclarationSyntax declarationSyntax)
        {
            var exceptions = GetExceptions(declarationSyntax.Body?.ToFullString());

            if (exceptions.Any())
            {
                foreach (var exception in exceptions)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateExceptionNodes(exception));
                }
            }
            return list;
        }

        /// <summary> Withs the existing. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="xmlNodeName"> The xml node name. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithExisting(this SyntaxList<XmlNodeSyntax> list, CSharpSyntaxNode declarationSyntax, string xmlNodeName)
        {
            var remarks = declarationSyntax.GetElementSyntax(xmlNodeName);
            if (remarks != null)
            {
                list = list.AddRange(DocumentationHeaderHelper.WrapElementSyntaxInCommentSyntax(remarks));
            }
            return list;
        }

        /// <summary> Withs the parameters. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithParameters(this SyntaxList<XmlNodeSyntax> list, BaseMethodDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.ParameterList?.Parameters.Any() == true)
            {
                foreach (ParameterSyntax parameter in declarationSyntax.ParameterList.Parameters)
                {
                    string parameterComment = CommentHelper.CreateParameterComment(parameter);
                    list = list.AddRange(DocumentationHeaderHelper.CreateParameterPartNodes(parameter.Identifier.ValueText, parameterComment));
                }
            }
            return list;
        }

        internal static SyntaxList<XmlNodeSyntax> WithParameters(this SyntaxList<XmlNodeSyntax> list, TypeDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.ParameterList?.Parameters.Any() == true)
            {
                foreach (ParameterSyntax parameter in declarationSyntax.ParameterList.Parameters)
                {
                    string parameterComment = CommentHelper.CreateParameterComment(parameter);
                    list = list.AddRange(DocumentationHeaderHelper.CreateParameterPartNodes(parameter.Identifier.ValueText, parameterComment));
                }
            }
            return list;
        }

        /// <summary> Withs the property value types. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="includeValueNodeInProperties"> If true, include value node in properties. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithPropertyValueTypes(this SyntaxList<XmlNodeSyntax> list, BasePropertyDeclarationSyntax declarationSyntax, bool includeValueNodeInProperties)
        {
            if (includeValueNodeInProperties)
            {
                string returnComment = new ReturnCommentConstruction(declarationSyntax.Type, false).Comment;
                list = list.AddRange(DocumentationHeaderHelper.CreateValuePartNodes(returnComment));
            }
            return list;
        }

        /// <summary> With the return type. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithReturnType(this SyntaxList<XmlNodeSyntax> list, MethodDeclarationSyntax declarationSyntax)
        {
            string returnType = declarationSyntax.ReturnType.ToString();
            if (returnType != "void")
            {
                var commentConstructor = new ReturnCommentConstruction(declarationSyntax.ReturnType);
                string returnComment = commentConstructor.Comment;
                list = list.AddRange(DocumentationHeaderHelper.CreateReturnPartNodes(returnComment));
            }
            return list;
        }

        /// <summary> Withs the summary. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <param name="content"> The content. </param>
        /// <param name="preserveExistingSummaryText"> If true, preserve existing summary text. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithSummary(this SyntaxList<XmlNodeSyntax> list, CSharpSyntaxNode declarationSyntax, string content, bool preserveExistingSummaryText)
        {
            XmlNodeSyntax[] summaryNodes = null;
            if (preserveExistingSummaryText)
            {
                var summary = declarationSyntax.GetElementSyntax(SUMMARY);
                if (summary != null)
                {
                    summaryNodes = DocumentationHeaderHelper.WrapElementSyntaxInCommentSyntax(summary);
                }
            }
            if (summaryNodes == null)
            {
                summaryNodes = DocumentationHeaderHelper.CreateSummaryPartNodes(content);
            }
            list = list.AddRange(summaryNodes);
            return list;
        }

        /// <summary> With the type paramters. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithTypeParamters(this SyntaxList<XmlNodeSyntax> list, TypeDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (TypeParameterSyntax parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateTypeParameterPartNodes(parameter.Identifier.ValueText));
                }
            }
            return list;
        }

        /// <summary> With the type paramters. </summary>
        /// <param name="list"> The list. </param>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> <![CDATA[SyntaxList<XmlNodeSyntax>]]> </returns>
        internal static SyntaxList<XmlNodeSyntax> WithTypeParamters(this SyntaxList<XmlNodeSyntax> list, MethodDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (TypeParameterSyntax parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    list = list.AddRange(DocumentationHeaderHelper.CreateTypeParameterPartNodes(parameter.Identifier.ValueText));
                }
            }
            return list;
        }

        #endregion Builders

        /// <summary> The reg ex. </summary>
        private static readonly Regex regEx = new Regex(@"throw\s+new\s+\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary> type param regex. </summary>
        private static readonly Regex typeParamRegex = new Regex(@"""\w+""");

        /// <summary> Creates the comment exterior. </summary>
        /// <returns> A SyntaxTriviaList. </returns>
        private static SyntaxTriviaList CreateCommentExterior()
        {
            return SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("///"));
        }

        /// <summary> Creates the line end text syntax. </summary>
        /// <returns> A XmlTextSyntax. </returns>
        private static XmlTextSyntax CreateLineEndTextSyntax()
        {
            /*
                /// <summary> The code fix provider. </summary>
                /// [0]
            */

            // [0] end line token.
            SyntaxToken xmlTextNewLineToken = CreateNewLineToken();
            XmlTextSyntax xmlText = SyntaxFactory.XmlText(xmlTextNewLineToken);
            return xmlText;
        }

        /// <summary> Creates the line start text syntax. </summary>
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

        /// <summary> Creates the new line token. </summary>
        /// <returns> A SyntaxToken. </returns>
        private static SyntaxToken CreateNewLineToken()
        {
            return SyntaxFactory.XmlTextNewLine(Environment.NewLine, false);
        }

        /// <summary> Create the return part nodes. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
        private static XmlNodeSyntax[] CreateReturnPartNodes(string content)
        {
            ///[0] <returns></returns>[1][2]

            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            var returnElement = CreateReturnElementSyntax(content);

            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, returnElement, lineEndText };
        }

        /// <summary> Creates the summary part nodes. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
        private static XmlNodeSyntax[] CreateSummaryPartNodes(string content)
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

        /// <summary> Create the type parameter part nodes. </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
        private static XmlNodeSyntax[] CreateTypeParameterPartNodes(string parameterName)
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

        /// <summary> Creates the value part nodes. </summary>
        /// <param name="content"> The content. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
        private static XmlNodeSyntax[] CreateValuePartNodes(string content)
        {
            ///[0] <value></value>[1][2]

            XmlTextSyntax lineStartText = CreateLineStartTextSyntax();

            var returnElement = CreateReturnElementSyntax(content, "value");

            XmlTextSyntax lineEndText = CreateLineEndTextSyntax();

            return new XmlNodeSyntax[] { lineStartText, returnElement, lineEndText };
        }

        /// <summary> Wrap element syntax in comment syntax. </summary>
        /// <param name="element"> The element. </param>
        /// <returns> An array of XmlNodeSyntaxes </returns>
        private static XmlNodeSyntax[] WrapElementSyntaxInCommentSyntax(XmlElementSyntax element)
        {
            /*
                 ///[0] <summary>
                 /// The code fix provider.
                 /// </summary>[1] [2]
             */

            // [0] " " + leading comment exterior trivia
            XmlTextSyntax xmlText0 = CreateLineStartTextSyntax();

            // [2] new line
            XmlTextSyntax xmlText1 = CreateLineEndTextSyntax();

            var nodes = new XmlNodeSyntax[] { xmlText0, element, xmlText1 };
            return nodes;
        }
    }
}
