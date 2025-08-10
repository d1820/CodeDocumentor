using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    /// <summary>
    ///  The documentation header helper.
    /// </summary>
    public class DocumentationHeaderHelper
    {
        private readonly Regex _regEx = new Regex(@"throw\s+new\s+\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex _regExInline = new Regex(@"(\w+Exception)\.Throw\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex _regExParseXmlElement = new Regex(@"<(.*?)\s(\w*)=""(.*?)""\s*/>", RegexOptions.IgnoreCase);
        private IOptionsService _optionsService;

        public DocumentationHeaderHelper(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        /// <summary>
        ///  Has analyzer exclusion.
        /// </summary>
        /// <param name="node"> The node. </param>
        /// <param name="recursive"> If true, recursive. </param>
        /// <returns> A bool. </returns>
        public bool HasAnalyzerExclusion(SyntaxNode node, bool recursive = true, List<AttributeSyntax> attributes = null)
        {
            if (node == null)
            {
                return false;
            }
            if (attributes == null)
            {
                attributes = new List<AttributeSyntax>();
            }

            if (node is MemberDeclarationSyntax memSyntax)
            {
                attributes.AddRange(GetAttributes(memSyntax));
            }

            if (node is CompilationUnitSyntax compSyntax)
            {
                attributes.AddRange(GetAttributes(compSyntax));
            }

            var hasExclusion = attributes.Any();
            return !hasExclusion && recursive ? HasAnalyzerExclusion(node.Parent, recursive, attributes) : hasExclusion;
        }

        internal XmlEmptyElementSyntax CreateElementWithAttributeSyntax(string elementName, string attributeName, string attributeValue)
        {
            return SyntaxFactory.XmlEmptyElement(
               SyntaxFactory.XmlName(elementName), // Element name
               SyntaxFactory.SingletonList<XmlAttributeSyntax>( // Attributes
                   SyntaxFactory.XmlTextAttribute(attributeName, attributeValue)
               )
           );
        }

        /// <summary>
        ///  Creates the parameter element syntax.
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterContent"> The parameter content. </param>
        /// <returns> A XmlElementSyntax. </returns>
        internal XmlElementSyntax CreateParameterElementSyntax(string parameterName, string parameterContent)
        {
            var paramName = SyntaxFactory.XmlName("param");

            /// <param name="parameterName"> [0][1] </param>
            /// [2]
            // [0] -- param start tag with attribute
            var paramAttribute = SyntaxFactory.XmlNameAttribute(parameterName);
            var startTag = SyntaxFactory.XmlElementStartTag(paramName, SyntaxFactory.SingletonList<XmlAttributeSyntax>(paramAttribute));

            // [1] -- content
            var content = SyntaxFactory.XmlText(parameterContent);

            // [2] -- end tag
            var endTag = SyntaxFactory.XmlElementEndTag(paramName);
            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(content), endTag);
        }

        /// <summary>
        ///  Create the return element syntax.
        /// </summary>
        /// <param name="content"> The content. </param>
        /// <param name="xmlNodeName"> The XML node name. </param>
        /// <returns> A XmlNodeSyntax. </returns>
        internal XmlNodeSyntax CreateReturnElementSyntax(string content, string xmlNodeName = "returns")
        {
            var xmlName = SyntaxFactory.XmlName(xmlNodeName);
            /// <returns> [0]xxx[1] </returns>
            /// [2]
            var startTag = SyntaxFactory.XmlElementStartTag(xmlName);
            var endTag = SyntaxFactory.XmlElementEndTag(xmlName);

            var regex = $@"<{xmlNodeName}>(.+)<\/{xmlNodeName}>";

            var cleanContent = (content ?? string.Empty).Trim();
            var pluckedReturnElemement = Regex.Match(cleanContent, regex);

            cleanContent = pluckedReturnElemement.Success ?
                            pluckedReturnElemement.Groups.Count > 0 ?
                                pluckedReturnElemement.Groups[0].Value
                                : cleanContent
                            : cleanContent;

            var xmlParseResponse = new XmlInformation(cleanContent);

            if (xmlParseResponse.HasSeeCrefNode || xmlParseResponse.HasTypeParam)
            {
                var xmlNodes = ParseStringToXmlNodeSyntax(cleanContent);
                var list = new SyntaxList<XmlNodeSyntax>(xmlNodes);
                return SyntaxFactory.XmlElement(startTag, list, endTag);
            }

            if (xmlParseResponse.IsCData)
            {
                var cDataContentRegEx = new Regex(@"\[.*\[(.*?)\]");
                var cDataConentMatch = cDataContentRegEx.Match(cleanContent);
                var cDataContent = cleanContent;
                if (cDataConentMatch.Success && cDataConentMatch.Groups.Count > 0)
                {
                    cDataContent = cDataConentMatch.Groups[1].Value;
                }

                //Wrap any XML thats not a match above to CDATA
                var text1Token = SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(), cDataContent, cDataContent, SyntaxFactory.TriviaList());
                var tokens = SyntaxFactory.TokenList().Add(text1Token);
                var cdata = SyntaxFactory.XmlCDataSection(SyntaxFactory.Token(SyntaxKind.XmlCDataStartToken), tokens, SyntaxFactory.Token(SyntaxKind.XmlCDataEndToken));

                return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(cdata), endTag);
            }

            if (xmlParseResponse.IsGeneric)
            {
                //Wrap any XML thats not a match above to CDATA
                var text1Token = SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(), cleanContent, cleanContent, SyntaxFactory.TriviaList());
                var tokens = SyntaxFactory.TokenList().Add(text1Token);
                var cdata = SyntaxFactory.XmlCDataSection(SyntaxFactory.Token(SyntaxKind.XmlCDataStartToken), tokens, SyntaxFactory.Token(SyntaxKind.XmlCDataEndToken));

                return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(cdata), endTag);
            }

            return SyntaxFactory.XmlElement(startTag, SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(cleanContent)), endTag);
        }

        /// <summary>
        ///  Determines specific object name.
        /// </summary>
        /// <param name="specificType"> The specific type. </param>
        /// <param name="pluaralizeName"> Flag determines if name should be pluralized </param>
        /// <returns> The comment. </returns>
        internal string DetermineSpecificObjectName(TypeSyntax specificType, bool pluaralizeName = false, bool pluaralizeIdentifierType = true)
        {
            string value;
            switch (specificType)
            {
                case IdentifierNameSyntax identifierNameSyntax:
                    value = identifierNameSyntax.Identifier.ValueText.ApplyUserTranslations(_optionsService.WordMaps);
                    return pluaralizeIdentifierType ? Pluralizer.Pluralize(value) : value;

                case PredefinedTypeSyntax predefinedTypeSyntax:
                    value = predefinedTypeSyntax.Keyword.ValueText.ApplyUserTranslations(_optionsService.WordMaps);
                    return pluaralizeName ? Pluralizer.Pluralize(value) : value;

                case GenericNameSyntax genericNameSyntax:
                    value = genericNameSyntax.Identifier.ValueText.ApplyUserTranslations(_optionsService.WordMaps);
                    return pluaralizeName ? Pluralizer.Pluralize(value) : value;

                default:
                    return specificType.ToFullString().ApplyUserTranslations(_optionsService.WordMaps);
            }
        }

        /// <summary>
        ///  Determines started word.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="useProperCasing"> If true, use proper casing. </param>
        /// <returns> A string. </returns>
        internal string DetermineStartingWord(ReadOnlySpan<char> returnType, bool useProperCasing = true)
        {
            if (returnType.IsEmpty)
            {
                return string.Empty;
            }
            var str = returnType.ToString();
            //if the returnType already starts with a or an then just return
            if (str.StartsWith_A_An_And() || str.IsXml())
            {
                return string.Empty;
            }
            var vowelChars = new List<char>() { 'a', 'e', 'i', 'o', 'u' };
            return vowelChars.Contains(char.ToLower(returnType[0])) ? useProperCasing ? "An" : "an" : useProperCasing ? "A" : "a";
        }

        internal IEnumerable<string> GetExceptions(string textToSearch)
        {
            if (string.IsNullOrEmpty(textToSearch))
            {
                return Enumerable.Empty<string>();
            }

            List<string> exceptions = new List<string>();

            TryHelper.Try(() =>
            {
                exceptions.AddRange(_regEx.Matches(textToSearch).OfType<Match>()
                                                           .Select(m => m?.Groups[0]?.Value)
                                                           .ToList());
            }, nameof(GetExceptions), eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.EXCEPTION_BUILDER);

            TryHelper.Try(() =>
            {
                var exceptionsInline = _regExInline.Matches(textToSearch).OfType<Match>()
                                                               .Select(m => m?.Groups.Count == 1 ? m?.Groups[0]?.Value : m?.Groups[1]?.Value).ToArray();
                exceptions.AddRange(exceptionsInline);
            }, nameof(GetExceptions), eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.EXCEPTION_BUILDER);

            return exceptions.Distinct();
        }

        internal List<XmlNodeSyntax> ParseStringToXmlNodeSyntax(string cleanContent)
        {
            var xmlNodes = new List<XmlNodeSyntax>();
            TryHelper.Try(() =>
            {
                var (replacedString, tokens) = cleanContent.SwapXmlTokens();
                var parts = replacedString.Split(' ');
                for (var i = 0; i < parts.Length; i++)
                {
                    var p = parts[i];
                    if (p.StartsWith("{"))
                    {
                        var elementName = "missing";
                        TryHelper.Try(() =>
                        {
                            var swapStr = tokens[p];
                            var xmlParseMatch = _regExParseXmlElement.Match(swapStr);
                            if (xmlParseMatch.Success && xmlParseMatch.Groups.Count > 3)
                            {
                                elementName = xmlParseMatch.Groups[1].Value;
                                var attributeName = xmlParseMatch.Groups[2].Value;
                                var attributeValue = xmlParseMatch.Groups[3].Value;
                                xmlNodes.Add(CreateElementWithAttributeSyntax(elementName, attributeName, attributeValue));
                                xmlNodes.Add(SyntaxFactory.XmlText(" "));
                            }
                        }, nameof(ParseStringToXmlNodeSyntax), (_) =>
                        {
                            xmlNodes.Add(SyntaxFactory.XmlText($"TODO: Add {elementName} XML"));
                            xmlNodes.Add(SyntaxFactory.XmlText(" "));
                        }, eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.XML_STRING_PARSER);
                    }
                    else
                    {
                        xmlNodes.Add(SyntaxFactory.XmlText(p));
                        xmlNodes.Add(SyntaxFactory.XmlText(" "));
                    }
                }
                xmlNodes.RemoveAt(xmlNodes.Count - 1);
            }, nameof(ParseStringToXmlNodeSyntax), (_) =>
            {
                xmlNodes.Clear();
                xmlNodes.Add(SyntaxFactory.XmlText(""));
            }, eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.XML_STRING_PARSER);

            return xmlNodes;
        }

        private IEnumerable<AttributeSyntax> GetAttributes(CompilationUnitSyntax node)
        {
            if (node == null)
            {
                return new SyntaxList<AttributeSyntax>();
            }

            var attrs = node.AttributeLists.SelectMany(w => w.Attributes);
            return attrs.Where(w => w.ArgumentList != null)
                         .SelectMany(w => w.ArgumentList.Arguments
                                .Where(ss => ss.Expression.IsKind(SyntaxKind.StringLiteralExpression) && ss.Expression.ToString().Contains(Constants.EXCLUSION_CATEGORY))
                                .Select(_ => w));
        }

        private IEnumerable<AttributeSyntax> GetAttributes(MemberDeclarationSyntax node)
        {
            if (node == null)
            {
                return new SyntaxList<AttributeSyntax>();
            }

            var attrs = node.AttributeLists.SelectMany(w => w.Attributes);
            return attrs.Where(w => w.ArgumentList != null)
                         .SelectMany(w => w.ArgumentList.Arguments
                                .Where(ss => ss.Expression.IsKind(SyntaxKind.StringLiteralExpression) && ss.Expression.ToString().Contains(Constants.EXCLUSION_CATEGORY))
                                .Select(_ => w));
        }
    }

    public static class SyntaxExtensions
    {
        /// <summary>
        ///  Checks if is dictionary.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsDictionary(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Dictionary");
        }

        public static bool IsDictionary(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Dictionary");
        }

        /// <summary>
        ///  Checks if is generic action result.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsGenericActionResult(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("ActionResult", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        public static bool IsGenericValueTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("ValueTask", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }

        /// <summary>
        ///  Checks if is list.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        public static bool PropertyHasSetter(this PropertyDeclarationSyntax declarationSyntax)
        {
            var hasSetter = false;

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

        /// <summary>
        ///  Checks if is list.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsList(this TypeSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.ToString();

            return genericTypeStr.Contains("Enumerable") || genericTypeStr.Contains("List") || genericTypeStr.Contains("Collection");
        }

        public static bool IsPropertyReturnTypeBool(this PropertyDeclarationSyntax declarationSyntax)
        {
            var isBoolean = false;
            if (declarationSyntax.Type.IsKind(SyntaxKind.PredefinedType))
            {
                isBoolean = ((PredefinedTypeSyntax)declarationSyntax.Type).Keyword.IsKind(SyntaxKind.BoolKeyword);
            }
            else if (declarationSyntax.Type.IsKind(SyntaxKind.NullableType))
            {
                if (((NullableTypeSyntax)declarationSyntax.Type).ElementType is PredefinedTypeSyntax returnType)
                {
                    isBoolean = returnType.ToString().IndexOf("bool", StringComparison.OrdinalIgnoreCase) > -1;
                }
            }

            return isBoolean;
        }

        /// <summary>
        ///  Checks if is read only collection.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this GenericNameSyntax nameSyntax)
        {
            return nameSyntax.Identifier.ValueText.Contains("ReadOnlyCollection");
        }

        /// <summary>
        ///  Checks if is read only collection.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsReadOnlyCollection(this TypeSyntax nameSyntax)
        {
            return nameSyntax.ToString().Contains("ReadOnlyCollection");
        }

        /// <summary>
        ///  Checks if is task.
        /// </summary>
        /// <param name="nameSyntax"> The name syntax. </param>
        /// <returns> A bool. </returns>
        public static bool IsTask(this GenericNameSyntax nameSyntax)
        {
            var genericTypeStr = nameSyntax.Identifier.ValueText;

            return genericTypeStr.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1 && nameSyntax.TypeArgumentList?.Arguments.Any() == true;
        }
        /// <summary>
        ///  Has summary.
        /// </summary>
        /// <param name="syntax"> The syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool HasSummary(this CSharpSyntaxNode syntax)
        {
            return syntax.HasLeadingTrivia && syntax.GetLeadingTrivia().Any(a => a.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || a.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia));
        }

        /// <summary>
        ///  Checks if is private.
        /// </summary>
        /// <param name="declarationSyntax"> The declaration syntax. </param>
        /// <returns> A bool. </returns>
        internal static bool IsPrivate(this BaseMethodDeclarationSyntax declarationSyntax)
        {
            var isPrivate = false;
            if (declarationSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                isPrivate = true;
            }
            return isPrivate;
        }

        /// <summary>
        ///  Gets the element syntax.
        /// </summary>
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
                }
            }
            return null;
        }


        /// <summary>
        ///  Upserts the leading trivia.
        /// </summary>
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
            return existingIndex < 0
                ? leadingTrivia.Insert(leadingTrivia.Count - 1, SyntaxFactory.Trivia(commentTrivia))
                : leadingTrivia.Replace(leadingTrivia[existingIndex], SyntaxFactory.Trivia(commentTrivia));
        }
    }
}
