using CodeDocumentor.Common.Builders;
using CodeDocumentor.Common.Helper;
using CodeDocumentor.Common.Interfaces;
using CodeDocumentor.Common.Managers;

namespace CodeDocumentor.Common.Locators
{
    public static class ServiceLocator
    {
        public static ICommentBuilderService CommentBuilderService { get; set; }
        public static DocumentationHeaderHelper DocumentationHeaderHelper { get; } = new DocumentationHeaderHelper();
        public static IEventLogger Logger { get; set; }
        public static ISettingService SettingService { get; set; }
        public static CommentHelper CommentHelper { get; } = new CommentHelper();
        public static GenericCommentManager GenericCommentManager { get; } = new GenericCommentManager();
        public static DocumentationBuilder DocumentationBuilder => new DocumentationBuilder();

    }
}
