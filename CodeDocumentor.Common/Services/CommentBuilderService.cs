using System;
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
        private readonly IBaseSettings _settings;

        public CommentBuilderService(IEventLogger eventLogger, IBaseSettings settings)
        {
            _eventLogger = eventLogger;
            _settings = settings;
        }

        public string AddDocumentation(string fileContents)
        {
            var tree = CSharpSyntaxTree.ParseText(fileContents);
            var root = tree.GetRoot();

            // Follow the same pattern as RegisterFileCodeFixesAsync in BaseCodeFixProvider
            var _nodesTempToReplace = new Dictionary<CSharpSyntaxNode, CSharpSyntaxNode>();

            // Order Matters - same order as in BaseCodeFixProvider.RegisterFileCodeFixesAsync
            var neededCommentCount = 0;
            neededCommentCount += BuildPropertyComments(_settings, Constants.DiagnosticIds.PROPERTY_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildConstructorComments(_settings, Constants.DiagnosticIds.CONSTRUCTOR_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildEnumComments(_settings, Constants.DiagnosticIds.ENUM_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildFieldComments(_settings, Constants.DiagnosticIds.FIELD_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildMethodComments(_settings, Constants.DiagnosticIds.METHOD_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildEventFieldComments(_settings, Constants.DiagnosticIds.EVENT_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildEventComments(_settings, Constants.DiagnosticIds.EVENT_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildDelegateComments(_settings, Constants.DiagnosticIds.DELEGATE_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildIndexerComments(_settings, Constants.DiagnosticIds.INDEXER_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildDestructorComments(_settings, Constants.DiagnosticIds.DESTRUCTOR_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildOperatorComments(_settings, Constants.DiagnosticIds.OPERATOR_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildConversionOperatorComments(_settings, Constants.DiagnosticIds.CONVERSION_OPERATOR_DIAGNOSTIC_ID, root, _nodesTempToReplace);

            // Replace nodes from first batch
            root = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) => _nodesTempToReplace[n1]);
            _nodesTempToReplace.Clear();

            // Second batch - same order as in BaseCodeFixProvider.RegisterFileCodeFixesAsync
            neededCommentCount += BuildInterfaceComments(_settings, Constants.DiagnosticIds.INTERFACE_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildComments(_settings, Constants.DiagnosticIds.CLASS_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildRecordComments(_settings, Constants.DiagnosticIds.RECORD_DIAGNOSTIC_ID, root, _nodesTempToReplace);
            neededCommentCount += BuildStructComments(_settings, Constants.DiagnosticIds.STRUCT_DIAGNOSTIC_ID, root, _nodesTempToReplace);

            // Final replacement
            var newRoot = root.ReplaceNodes(_nodesTempToReplace.Keys, (n1, n2) => _nodesTempToReplace[n1]);

            return newRoot.GetText().ToString();
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

        public int BuildComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public ClassDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, ClassDeclarationSyntax declarationSyntax)
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

        public int BuildPropertyComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public PropertyDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, PropertyDeclarationSyntax declarationSyntax)
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

        public int BuildConstructorComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public ConstructorDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, ConstructorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateConstructorDocumentationCommentTrivia(settings, declarationSyntax);
            var newDeclaration = declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
            return newDeclaration;
        }

        private DocumentationCommentTriviaSyntax CreateConstructorDocumentationCommentTrivia(IBaseSettings settings, ConstructorDeclarationSyntax declarationSyntax)
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

        public int BuildEnumComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.EnumDeclaration)).OfType<EnumDeclarationSyntax>().ToArray();
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

        public EnumDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, EnumDeclarationSyntax declarationSyntax)
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

        public int BuildFieldComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public FieldDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, FieldDeclarationSyntax declarationSyntax)
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

        public int BuildMethodComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public MethodDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, MethodDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var commentTrivia = CreateMethodDocumentationCommentTrivia(settings, declarationSyntax);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public MethodDeclarationSyntax BuildNewDeclaration(MethodDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }

        private DocumentationCommentTriviaSyntax CreateMethodDocumentationCommentTrivia(IBaseSettings settings, MethodDeclarationSyntax declarationSyntax)
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

        public int BuildInterfaceComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public InterfaceDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, InterfaceDeclarationSyntax declarationSyntax)
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

        public int BuildRecordComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
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

        public RecordDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, RecordDeclarationSyntax declarationSyntax)
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

        #region Event Field Methods
        public int BuildEventFieldComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildEventFieldComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildEventFieldComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.EventFieldDeclaration)).OfType<EventFieldDeclarationSyntax>().ToArray();
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

        public EventFieldDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, EventFieldDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var variable = declarationSyntax.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var comment = ServiceLocator.CommentHelper.CreateEventComment(variable?.Identifier.ValueText, settings.WordMaps);
            var summaryNodes = ServiceLocator.DocumentationBuilder.WithSummary(comment).Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, summaryNodes);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public EventFieldDeclarationSyntax BuildNewDeclaration(EventFieldDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Event (explicit) Methods
        public int BuildEventComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildEventComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildEventComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.EventDeclaration)).OfType<EventDeclarationSyntax>().ToArray();
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

        public EventDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, EventDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var comment = ServiceLocator.CommentHelper.CreateEventComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var summaryNodes = ServiceLocator.DocumentationBuilder.WithSummary(comment).Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, summaryNodes);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public EventDeclarationSyntax BuildNewDeclaration(EventDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Delegate Methods
        public int BuildDelegateComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildDelegateComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildDelegateComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.DelegateDeclaration)).OfType<DelegateDeclarationSyntax>().ToArray();
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

        public DelegateDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, DelegateDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var comment = ServiceLocator.CommentHelper.CreateDelegateComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithReturnType(declarationSyntax, settings.UseNaturalLanguageForReturnNode, settings.TryToIncludeCrefsForReturnTypes, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public DelegateDeclarationSyntax BuildNewDeclaration(DelegateDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Struct Methods
        public int BuildStructComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildStructComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildStructComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.StructDeclaration)).OfType<StructDeclarationSyntax>().ToArray();
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

        public StructDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, StructDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var comment = ServiceLocator.CommentHelper.CreateStructComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithTypeParamters(declarationSyntax)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public StructDeclarationSyntax BuildNewDeclaration(StructDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Indexer Methods
        public int BuildIndexerComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildIndexerComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildIndexerComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.IndexerDeclaration)).OfType<IndexerDeclarationSyntax>().ToArray();
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

        public IndexerDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, IndexerDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var hasSetter = declarationSyntax.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) == true;
            var comment = ServiceLocator.CommentHelper.CreateIndexerComment(hasSetter, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;

            var returnOptions = new ReturnTypeBuilderOptions
            {
                TryToIncludeCrefsForReturnTypes = settings.TryToIncludeCrefsForReturnTypes,
                GenerateReturnStatement = settings.IncludeValueNodeInProperties,
                ReturnGenericTypeAsFullString = false,
                IncludeStartingWordInText = true,
                UseProperCasing = true
            };

            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithPropertyValueTypes(declarationSyntax, returnOptions, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public IndexerDeclarationSyntax BuildNewDeclaration(IndexerDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Destructor Methods
        public int BuildDestructorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildDestructorComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildDestructorComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.DestructorDeclaration)).OfType<DestructorDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
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

        public DestructorDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, DestructorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var comment = ServiceLocator.CommentHelper.CreateDestructorComment(declarationSyntax.Identifier.ValueText, settings.WordMaps);
            var summaryNodes = ServiceLocator.DocumentationBuilder.WithSummary(comment).Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, summaryNodes);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public DestructorDeclarationSyntax BuildNewDeclaration(DestructorDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Operator Methods
        public int BuildOperatorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildOperatorComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildOperatorComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.OperatorDeclaration)).OfType<OperatorDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
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

        public OperatorDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, OperatorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var comment = ServiceLocator.CommentHelper.CreateOperatorComment(declarationSyntax.OperatorToken.ValueText, settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithReturnType(declarationSyntax, settings.UseNaturalLanguageForReturnNode, settings.TryToIncludeCrefsForReturnTypes, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public OperatorDeclarationSyntax BuildNewDeclaration(OperatorDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Conversion Operator Methods
        public int BuildConversionOperatorComments(string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            return BuildConversionOperatorComments(_settings, diagnosticId, root, nodesToReplace);
        }

        public int BuildConversionOperatorComments(IBaseSettings settings, string diagnosticId, SyntaxNode root, Dictionary<CSharpSyntaxNode, CSharpSyntaxNode> nodesToReplace)
        {
            var declarations = root.DescendantNodes().Where(w => w.IsKind(SyntaxKind.ConversionOperatorDeclaration)).OfType<ConversionOperatorDeclarationSyntax>().ToArray();
            var neededCommentCount = 0;
            TryHelper.Try(() =>
            {
                foreach (var declarationSyntax in declarations)
                {
                    if (declarationSyntax.HasSummary())
                    {
                        continue;
                    }
                    if (settings.IsEnabledForPublicMembersOnly && PrivateMemberVerifier.IsPrivateMember(declarationSyntax))
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

        public ConversionOperatorDeclarationSyntax BuildNewDeclaration(IBaseSettings settings, ConversionOperatorDeclarationSyntax declarationSyntax)
        {
            var leadingTrivia = declarationSyntax.GetLeadingTrivia();
            var isImplicit = declarationSyntax.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword);
            var comment = ServiceLocator.CommentHelper.CreateConversionOperatorComment(isImplicit, declarationSyntax.Type.ToString(), settings.WordMaps);
            var builder = ServiceLocator.DocumentationBuilder;
            var list = builder.WithSummary(declarationSyntax, comment, settings.PreserveExistingSummaryText)
                        .WithParameters(declarationSyntax, settings.WordMaps)
                        .WithExisting(declarationSyntax, Constants.REMARKS)
                        .WithExisting(declarationSyntax, Constants.EXAMPLE)
                        .Build();
            var commentTrivia = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, list);
            return declarationSyntax.WithLeadingTrivia(leadingTrivia.UpsertLeadingTrivia(commentTrivia));
        }

        public ConversionOperatorDeclarationSyntax BuildNewDeclaration(ConversionOperatorDeclarationSyntax declarationSyntax)
        {
            return BuildNewDeclaration(_settings, declarationSyntax);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Determines if a syntax node is documentable (can have XML documentation comments)
        /// </summary>
        public bool IsDocumentableNode(SyntaxNode node)
        {
            switch (node)
            {
                case ClassDeclarationSyntax _:
                case InterfaceDeclarationSyntax _:
                case RecordDeclarationSyntax _:
                case StructDeclarationSyntax _:
                case EnumDeclarationSyntax _:
                case MethodDeclarationSyntax _:
                case PropertyDeclarationSyntax _:
                case ConstructorDeclarationSyntax _:
                case FieldDeclarationSyntax _:
                case EventFieldDeclarationSyntax _:
                case EventDeclarationSyntax _:
                case DelegateDeclarationSyntax _:
                case IndexerDeclarationSyntax _:
                case DestructorDeclarationSyntax _:
                case OperatorDeclarationSyntax _:
                case ConversionOperatorDeclarationSyntax _:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Calculates the number of non-empty lines in the XML documentation comments that precede the specified syntax
        /// node.
        /// </summary>
        /// <remarks>Only lines within single-line or multi-line XML documentation comments are counted.
        /// Blank or whitespace-only lines are excluded from the count.</remarks>
        /// <param name="node">The syntax node whose leading XML documentation comment lines are to be counted.</param>
        /// <returns>The number of non-empty lines found in the single-line or multi-line XML documentation comments immediately
        /// preceding the specified node.</returns>
        public int GetDocumentationLineCount(SyntaxNode node)
        {
            return node.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                .SelectMany(t => t.ToFullString().Split(new[] { '\n' }, StringSplitOptions.None).Select(line => line.Trim()))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Count();
        }

        /// <summary>
        /// Builds documentation for any supported syntax node type
        /// </summary>
        public SyntaxNode BuildNewDocumentationNode(SyntaxNode node)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classNode:
                    return BuildNewDeclaration(classNode);
                case InterfaceDeclarationSyntax interfaceNode:
                    return BuildNewDeclaration(interfaceNode);
                case RecordDeclarationSyntax recordNode:
                    return BuildNewDeclaration(recordNode);
                case EnumDeclarationSyntax enumNode:
                    return BuildNewDeclaration(enumNode);
                case MethodDeclarationSyntax methodNode:
                    return BuildNewDeclaration(methodNode);
                case PropertyDeclarationSyntax propertyNode:
                    return BuildNewDeclaration(propertyNode);
                case ConstructorDeclarationSyntax constructorNode:
                    return BuildNewDeclaration(constructorNode);
                case FieldDeclarationSyntax fieldNode:
                    return BuildNewDeclaration(fieldNode);
                case StructDeclarationSyntax structNode:
                    return BuildNewDeclaration(structNode);
                case EventFieldDeclarationSyntax eventFieldNode:
                    return BuildNewDeclaration(eventFieldNode);
                case EventDeclarationSyntax eventNode:
                    return BuildNewDeclaration(eventNode);
                case DelegateDeclarationSyntax delegateNode:
                    return BuildNewDeclaration(delegateNode);
                case IndexerDeclarationSyntax indexerNode:
                    return BuildNewDeclaration(indexerNode);
                case DestructorDeclarationSyntax destructorNode:
                    return BuildNewDeclaration(destructorNode);
                case OperatorDeclarationSyntax operatorNode:
                    return BuildNewDeclaration(operatorNode);
                case ConversionOperatorDeclarationSyntax conversionNode:
                    return BuildNewDeclaration(conversionNode);
                default:
                    return null;
            }
        }
        #endregion
    }
}
