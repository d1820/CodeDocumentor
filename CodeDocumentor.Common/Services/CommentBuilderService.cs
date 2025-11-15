using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Locators;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Common.Services
{
    public class CommentBuilderService : ICommentBuilderService
    {
        private readonly IEventLogger _eventLogger;
        private readonly ISettings _settings;

        public CommentBuilderService(IEventLogger eventLogger, ISettings settings)
        {
            _eventLogger = eventLogger;
            _settings = settings;
        }
        
        #region Class Methods
        /// <summary>
        ///  Builds the comments. This is only used in the file level fixProvider.
        /// </summary>
        /// <param name="root"> The root. </param>
        /// <param name="nodesToReplace"> The nodes to replace. </param>
        public int BuildComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly
                        && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary()) //if the class has comments dont redo it. User should update manually
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public ClassDeclarationSyntax BuildNewDeclaration(ISettings settings, ClassDeclarationSyntax declarationSyntax)
        {
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateClassComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                            .WithTypeParamters(declarationSyntax)
                            .WithParameters(declarationSyntax, settings.WordMaps)
                            .WithExisting(declarationSyntax, Constants.REMARKS)
                            .WithExisting(declarationSyntax, Constants.EXAMPLE)
                            .Build();

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            //append to any existing leading trivia [attributes, decorators, etc)
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();

            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public ClassDeclarationSyntax BuildNewDeclaration(ClassDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Property Methods
        public int BuildPropertyComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildPropertyComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildPropertyComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.PropertyDeclaration)).OfType<PropertyDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public PropertyDeclarationSyntax BuildNewDeclaration(ISettings settings, PropertyDeclarationSyntax declarationSyntax)
        {
            var isBoolean = declarationSyntax.IsPropertyReturnTypeBool();
            var hasSetter = declarationSyntax.PropertyHasSetter();

            var commentHelper = ServiceLocator.CommentHelper;
            var propertyComment = commentHelper.CreatePropertyComment(declarationSyntax.Identifier.ValueText, isBoolean,
                                                                        hasSetter, settings.ExcludeAsyncSuffix, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;

            var returnOptions = new ReturnTypeBuilderOptions
            {
                TryToIncludeCrefsForReturnTypes = settings.TryToIncludeCrefsForReturnTypes,
                GenerateReturnStatement = settings.IncludeValueNodeInProperties,
                ReturnGenericTypeAsFullString = false,
                IncludeStartingWordInText = true,
                UseProperCasing = true
            };
            var list = builder.WithSummary(declarationSyntax, propertyComment, settings.PreserveExistingSummaryText)
                        .WithPropertyValueTypes(declarationSyntax, returnOptions, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public PropertyDeclarationSyntax BuildNewDeclaration(PropertyDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Constructor Methods
        public int BuildConstructorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildConstructorComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildConstructorComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ConstructorDeclaration)).OfType<ConstructorDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public ConstructorDeclarationSyntax BuildNewDeclaration(ISettings settings, ConstructorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateConstructorDocumentationCommentTrivia(settings, declarationSyntax);
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        private DocumentationCommentTriviaSyntax CreateConstructorDocumentationCommentTrivia(ISettings settings, ConstructorDeclarationSyntax declarationSyntax)
        {
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateConstructorComment(declarationSyntax.Identifier.ValueText, declarationSyntax.IsPrivate(), settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();

            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }

        public ConstructorDeclarationSyntax BuildNewDeclaration(ConstructorDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Enum Methods
        public int BuildEnumComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildEnumComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildEnumComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.EnumDeclaration)).OfType<EnumDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public EnumDeclarationSyntax BuildNewDeclaration(ISettings settings, EnumDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateEnumComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);

            var builder = ServiceLocator.DocumentationBuilder;

            var summaryNodes = builder.WithSummary(comment).Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, summaryNodes);
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public EnumDeclarationSyntax BuildNewDeclaration(EnumDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Field Methods
        public int BuildFieldComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildFieldComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildFieldComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.FieldDeclaration)).OfType<FieldDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public FieldDeclarationSyntax BuildNewDeclaration(ISettings settings, FieldDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var field = declarationSyntax.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateFieldComment(field?.Identifier.ValueText, settings.ExcludeAsyncSuffix, settings.WordMaps);

            var builder = ServiceLocator.DocumentationBuilder;

            var summaryNodes = builder.WithSummary(comment).Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, summaryNodes);
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public FieldDeclarationSyntax BuildNewDeclaration(FieldDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Method Methods
        public int BuildMethodComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildMethodComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildMethodComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.MethodDeclaration)).OfType<MethodDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (
                       !declarationSyntax.IsOwnedByInterface() &&
                       settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax)
                    )
                    {
                        continue;
                    }
                    //if method is already commented dont redo it, user should update methods individually
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public MethodDeclarationSyntax BuildNewDeclaration(ISettings settings, MethodDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateMethodDocumentationCommentTrivia(settings, declarationSyntax);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }

        private DocumentationCommentTriviaSyntax CreateMethodDocumentationCommentTrivia(ISettings settings, MethodDeclarationSyntax declarationSyntax)
        {
            var commentHelper = ServiceLocator.CommentHelper;
            var summaryText = commentHelper.CreateMethodComment(declarationSyntax.Identifier.ValueText,
                                                                declarationSyntax.ReturnType,
                                                               settings.UseToDoCommentsOnSummaryError,
                                                               settings.TryToIncludeCrefsForReturnTypes,
                                                               settings.ExcludeAsyncSuffix,
                                                               settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;

            var list = builder.WithSummary(declarationSyntax, summaryText, settings.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithExceptionTypes(declarationSyntax)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .WithReturnType(declarationSyntax, settings.UseNaturalLanguageForReturnNode, settings.TryToIncludeCrefsForReturnTypes, settings.WordMaps)
                        .Build();

            return SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
        }
        #endregion

        #region Interface Methods
        public int BuildInterfaceComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildInterfaceComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildInterfaceComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.InterfaceDeclaration)).OfType<InterfaceDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public InterfaceDeclarationSyntax BuildNewDeclaration(ISettings settings, InterfaceDeclarationSyntax declarationSyntax)
        {
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateInterfaceComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .Build();

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public InterfaceDeclarationSyntax BuildNewDeclaration(InterfaceDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Record Methods
        public int BuildRecordComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildRecordComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildRecordComments(ISettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.RecordDeclaration)).OfType<RecordDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (settings.IsEnabledForPublicMembersOnly
                        && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
                    {
                        continue;
                    }
                    if (declarationSyntax.HasSummary()) //if record already has comments dont redo it. User should update this manually
                    {
                        continue;
                    }
                    var newDeclaration = BuildNewDeclaration(settings, declarationSyntax);
                    nodesToReplace.TryAdd(declarationSyntax, newDeclaration);
                    neededCommentCount++;
                }
            }, diagnosticId, _eventLogger, eventId: Constants.EventIds.FIXER, category: Constants.EventIds.Categories.BUILD_COMMENTS);
            return neededCommentCount;
        }

        public RecordDeclarationSyntax BuildNewDeclaration(ISettings settings, RecordDeclarationSyntax declarationSyntax)
        {
            var commentHelper = ServiceLocator.CommentHelper;
            var comment = commentHelper.CreateRecordComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;

            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();

            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);

            //append to any existing leading trivia [attributes, decorators, etc)
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();

            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        public RecordDeclarationSyntax BuildNewDeclaration(RecordDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion
    }
}
