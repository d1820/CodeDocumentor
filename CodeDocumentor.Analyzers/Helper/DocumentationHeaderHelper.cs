using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Analyzers.Locators;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Extensions;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Analyzers.Helper
{
    /// <summary>
    ///  The documentation header helper.
    /// </summary>
    public class DocumentationHeaderHelper
    {
        private readonly Regex _regEx = new Regex(@"throw\s+new\s+\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex _regExInline = new Regex(@"(\w+Exception)\.Throw\w+", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly Regex _regExParseXmlElement = new Regex(@"<(.*?)\s(\w*)=""(.*?)""\s*/>", RegexOptions.IgnoreCase);
        private readonly IEventLogger _eventLogger = ServiceLocator.Logger;

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

        public XmlEmptyElementSyntax CreateElementWithAttributeSyntax(string elementName, string attributeName, string attributeValue)
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
        public XmlElementSyntax CreateParameterElementSyntax(string parameterName, string parameterContent)
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
        public XmlNodeSyntax CreateReturnElementSyntax(string content, string xmlNodeName = "returns")
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
        public string DetermineSpecificObjectName(TypeSyntax specificType, WordMap[] wordMaps, bool pluaralizeName = false, bool pluaralizeIdentifierType = true)
        {
            string value;
            switch (specificType)
            {
                case IdentifierNameSyntax identifierNameSyntax:
                    value = identifierNameSyntax.Identifier.ValueText.ApplyUserTranslations(wordMaps);
                    return pluaralizeIdentifierType ? Pluralizer.Pluralize(value) : value;

                case PredefinedTypeSyntax predefinedTypeSyntax:
                    value = predefinedTypeSyntax.Keyword.ValueText.ApplyUserTranslations(wordMaps);
                    return pluaralizeName ? Pluralizer.Pluralize(value) : value;

                case GenericNameSyntax genericNameSyntax:
                    value = genericNameSyntax.Identifier.ValueText.ApplyUserTranslations(wordMaps);
                    return pluaralizeName ? Pluralizer.Pluralize(value) : value;

                default:
                    return specificType.ToFullString().ApplyUserTranslations(wordMaps);
            }
        }

        /// <summary>
        ///  Determines started word.
        /// </summary>
        /// <param name="returnType"> The return type. </param>
        /// <param name="useProperCasing"> If true, use proper casing. </param>
        /// <returns> A string. </returns>
        public string DetermineStartingWord(ReadOnlySpan<char> returnType, bool useProperCasing = true)
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

        public IEnumerable<string> GetExceptions(string textToSearch)
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
            }, nameof(GetExceptions), _eventLogger, eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.EXCEPTION_BUILDER);

            TryHelper.Try(() =>
            {
                var exceptionsInline = _regExInline.Matches(textToSearch).OfType<Match>()
                                                               .Select(m => m?.Groups.Count == 1 ? m?.Groups[0]?.Value : m?.Groups[1]?.Value).ToArray();
                exceptions.AddRange(exceptionsInline);
            }, nameof(GetExceptions), _eventLogger, eventId: Constants.EventIds.HEADER_HELPER, category: Constants.EventIds.Categories.EXCEPTION_BUILDER);

            return exceptions.Distinct();
        }

        public List<XmlNodeSyntax> ParseStringToXmlNodeSyntax(string cleanContent)
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
                        }, nameof(ParseStringToXmlNodeSyntax), _eventLogger, (_) =>
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
            }, nameof(ParseStringToXmlNodeSyntax), _eventLogger, (_) =>
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
}
