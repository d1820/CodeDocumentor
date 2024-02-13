using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Constructors;
using CodeDocumentor.Helper;
using CodeDocumentor.Vsix2022;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Builders
{
    public class DocumentationBuilder
    {
        private List<XmlNodeSyntax> _list = new List<XmlNodeSyntax>();
        private XmlElementSyntax _currentElement;

        internal SyntaxList<XmlNodeSyntax> Build()
        {
            return new SyntaxList<XmlNodeSyntax>(_list);
        }

        internal DocumentationBuilder Reset()
        {
            _currentElement = null;
            return this;
        }

        internal DocumentationBuilder WithExceptionTypes(MethodDeclarationSyntax declarationSyntax)
        {
            var exceptions = DocumentationHeaderHelper.GetExceptions(declarationSyntax.Body?.ToFullString());

            if (exceptions.Any())
            {
                foreach (var exception in exceptions)
                {
                    var identity = SyntaxFactory.IdentifierName(exception.Replace("throw new", string.Empty).Trim());
                    CrefSyntax cref = SyntaxFactory.NameMemberCref(identity);
                    var exceptionElement = SyntaxFactory.XmlExceptionElement(cref);
                    Reset().WithTripleSlashSpace()
                          .WithElement(exceptionElement) //this already contains the rest of the /// for all the line <summary>...</summary>
                          .WithLineEndTextSyntax();

                    //_list = _list.AddRange(CreateExceptionNodes(exception));
                }
            }
            return this;
        }

        internal DocumentationBuilder WithExisting(CSharpSyntaxNode declarationSyntax, string xmlNodeName)
        {
            var remarks = declarationSyntax.GetElementSyntax(xmlNodeName);
            if (remarks != null)
            {
                Reset().WithTripleSlashSpace()
                           .WithElement(remarks)
                           .WithLineEndTextSyntax();
            }
            return this;
        }

        internal DocumentationBuilder WithParameters(BaseMethodDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.ParameterList?.Parameters.Any() == true)
            {
                foreach (var parameter in declarationSyntax.ParameterList.Parameters)
                {
                    var parameterComment = CommentHelper.CreateParameterComment(parameter);
                    var parameterElement = DocumentationHeaderHelper.CreateParameterElementSyntax(parameter.Identifier.ValueText, parameterComment);

                    Reset().WithTripleSlashSpace()
                                .WithElement(parameterElement)
                                .WithLineEndTextSyntax();

                    //_list = _list.AddRange(CreateParameterPartNodes(parameter.Identifier.ValueText, parameterComment));
                }
            }
            return this;
        }

        internal DocumentationBuilder WithParameters(TypeDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.ParameterList?.Parameters.Any() == true)
            {
                foreach (var parameter in declarationSyntax.ParameterList.Parameters)
                {
                    var parameterComment = CommentHelper.CreateParameterComment(parameter);

                    var parameterElement = DocumentationHeaderHelper.CreateParameterElementSyntax(parameter.Identifier.ValueText, parameterComment);

                    Reset().WithTripleSlashSpace()
                                .WithElement(parameterElement)
                                .WithLineEndTextSyntax();
                }
            }
            return this;
        }

        internal DocumentationBuilder WithPropertyValueTypes(BasePropertyDeclarationSyntax declarationSyntax, bool includeValueNodeInProperties)
        {
            if (includeValueNodeInProperties)
            {
                var options = new ReturnTypeBuilderOptions
                {
                    ReturnGenericTypeAsFullString = false,
                    //BuildWithPeriodAndPrefixForTaskTypes = false,
                    TryToIncludeCrefsForReturnTypes = true,
                    //IncludeReturnStatementInGeneralComments = true
                };
                var returnComment = new ReturnCommentConstruction(declarationSyntax.Type, options).Comment;
                var returnElement = DocumentationHeaderHelper.CreateReturnElementSyntax(returnComment);

                Reset().WithTripleSlashSpace()
                            .WithElement(returnElement) //this already contains the rest of the /// for all the line <summary>...</summary>
                            .WithLineEndTextSyntax();

            }
            return this;
        }

        internal DocumentationBuilder WithReturnType(MethodDeclarationSyntax declarationSyntax)
        {
            var returnType = declarationSyntax.ReturnType.ToString();
            if (returnType != "void")
            {
                var commentConstructor = new ReturnCommentConstruction(declarationSyntax.ReturnType);
                var returnComment = commentConstructor.Comment;
                var returnElement = DocumentationHeaderHelper.CreateReturnElementSyntax(returnComment);

                Reset().WithTripleSlashSpace()
                            .WithElement(returnElement) //this already contains the rest of the /// for all the line <summary>...</summary>
                            .WithLineEndTextSyntax();
            }
            return this;
        }

        internal DocumentationBuilder WithSummary(CSharpSyntaxNode declarationSyntax, string content, bool preserveExistingSummaryText)
        {
            if (preserveExistingSummaryText)
            {
                var summary = declarationSyntax.GetElementSyntax(Constants.SUMMARY);
                if (summary != null)
                {
                    Reset().WithTripleSlashSpace()
                        .WithElement(summary) //this already contains the rest of the /// for all the line <summary>...</summary>
                        .WithLineEndTextSyntax();
                }
            }
            else
            {
                Reset().WithTripleSlashSpace().WithStartingTag(Constants.SUMMARY).WithLineEndTextSyntax()
                       .WithTripleSlashSpace().WithContent(content).WithLineEndTextSyntax()
                       .WithTripleSlashSpace().WithEndingTag(Constants.SUMMARY).WithLineEndTextSyntax();
            }

            return this;
        }

        internal DocumentationBuilder WithSummary(string content)
        {
            Reset().WithTripleSlashSpace().WithStartingTag(Constants.SUMMARY).WithLineEndTextSyntax()
                        .WithTripleSlashSpace().WithContent(content).WithLineEndTextSyntax()
                        .WithTripleSlashSpace().WithEndingTag(Constants.SUMMARY).WithLineEndTextSyntax();

            return this;
        }

        internal DocumentationBuilder WithTypeParamters(TypeDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (var parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    var typeElement = DocumentationHeaderHelper.CreateElementWithAttributeSyntax("typeparam", "name", parameter.Identifier.ValueText);
                    Reset().WithTripleSlashSpace()
                            .WithElement(typeElement) //this already contains the rest of the /// for all the line <summary>...</summary>
                            .WithLineEndTextSyntax();
                }
            }
            return this;
        }

        internal DocumentationBuilder WithTypeParamters(MethodDeclarationSyntax declarationSyntax)
        {
            if (declarationSyntax?.TypeParameterList?.Parameters.Any() == true)
            {
                foreach (var parameter in declarationSyntax.TypeParameterList.Parameters)
                {
                    var typeElement = DocumentationHeaderHelper.CreateElementWithAttributeSyntax("typeparam", "name", parameter.Identifier.ValueText);
                    Reset().WithTripleSlashSpace()
                            .WithElement(typeElement) //this already contains the rest of the /// for all the line <summary>...</summary>
                            .WithLineEndTextSyntax();
                }
            }
            return this;
        }

        private DocumentationBuilder WithLineEndTextSyntax()
        {
            if (_currentElement != null)
            {
                var currentContent = _currentElement.Content;
                if (currentContent != null)
                {
                    currentContent = currentContent.Add(BuildNewLineToken());
                }
                _currentElement = _currentElement.WithContent(currentContent);
                return this;
            }
            /*
                /// <summary>
                ///  The code fix provider.
                /// </summary>[0]
            */

            // [0] end line token.
            var xmlText = BuildNewLineToken();
            _list.Add(xmlText);
            return this;

            XmlTextSyntax BuildNewLineToken()
            {
                var xmlTextNewLineToken = CreateNewLine();
                return SyntaxFactory.XmlText(xmlTextNewLineToken);
            }
        }

        private DocumentationBuilder WithTripleSlashSpace()
        {
            if (_currentElement != null)
            {
                var currentContent = _currentElement.Content;
                if (currentContent != null)
                {
                    currentContent = currentContent.Add(BuildTrippleSlashToken());
                }
                _currentElement = _currentElement.WithContent(currentContent);
                return this;
            }
            /*
                ///[0] <summary>
                /// The code fix provider.
                /// </summary>
            */

            // [0] " " + leading comment exterior trivia
            var xmlText0 = BuildTrippleSlashToken();
            _list.Add(xmlText0);
            return this;

            XmlTextSyntax BuildTrippleSlashToken()
            {
                var xmlText0Leading = SyntaxFactory.TriviaList(CreateTripleSlash());
                var xmlText0LiteralToken = SyntaxFactory.XmlTextLiteral(xmlText0Leading, " ", " ", SyntaxFactory.TriviaList());
                return SyntaxFactory.XmlText(xmlText0LiteralToken);
            }
        }

        private DocumentationBuilder WithStartingTag(string tagName)
        {
            var elementName = SyntaxFactory.XmlName(tagName);

            _currentElement = SyntaxFactory.XmlElement(elementName, new SyntaxList<XmlNodeSyntax>());
            return this;
        }

        private DocumentationBuilder WithEndingTag(string tagName)
        {
            var clone = _currentElement.Update(_currentElement.StartTag, _currentElement.Content, _currentElement.EndTag);
            _list.Add(clone);
            _currentElement = null;
            return this;
        }

        private DocumentationBuilder WithElement(XmlNodeSyntax element)
        {
            _list.Add(element);
            return this;
        }

        private DocumentationBuilder WithContent(string content)
        {
            if (_currentElement != null)
            {
                var currentContent = _currentElement.Content;
                if (currentContent != null)
                {
                    var contentLine = SyntaxFactory.XmlTextLiteral(SyntaxFactory.TriviaList(), content, content, SyntaxFactory.TriviaList());
                    var contentXml = SyntaxFactory.XmlText(contentLine);
                    currentContent = currentContent.Add(contentXml);
                }
                _currentElement = _currentElement.WithContent(currentContent);
            }
            return this;
        }

        private SyntaxToken CreateNewLine()
        {
            return SyntaxFactory.XmlTextNewLine(Constants.NEWLINE, false);
        }

        private SyntaxTrivia CreateTripleSlash()
        {
            return SyntaxFactory.DocumentationCommentExterior("///");
        }
    }
}
