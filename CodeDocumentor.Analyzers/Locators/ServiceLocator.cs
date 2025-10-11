using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Managers;
using CodeDocumentor.Common.Interfaces;

namespace CodeDocumentor.Analyzers.Locators
{
    public static class ServiceLocator
    {
        public static DocumentationHeaderHelper DocumentationHeaderHelper { get; } = new DocumentationHeaderHelper();
        public static IEventLogger Logger { get; set; }
        public static ISettingService SettingService { get; set; }
        public static CommentHelper CommentHelper { get; } = new CommentHelper();
        public static GenericCommentManager GenericCommentManager { get; } = new GenericCommentManager();
        public static DocumentationBuilder DocumentationBuilder => new DocumentationBuilder();

    }
}
