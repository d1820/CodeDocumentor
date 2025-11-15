using CodeDocumentor.Analyzers.Builders;
using CodeDocumentor.Analyzers.Helper;
using CodeDocumentor.Analyzers.Managers;
using CodeDocumentor.Analyzers.Services;
using CodeDocumentor.Common.Interfaces;

namespace CodeDocumentor.Analyzers.Locators
{
    public static class ServiceLocator
    {
        public static DocumentationHeaderHelper DocumentationHeaderHelper { get; } = new DocumentationHeaderHelper();
        public static IEventLogger Logger { get; set; } = new PreLoadLogger(); //this is a temp until the real logger can be set at package load time
        public static ISettingService SettingService { get; set; } = new PreLoadSettingService(); //this is a temp until the LoadAsync can finish with the real settings. but we need something due to order of events firing
        public static CommentHelper CommentHelper { get; } = new CommentHelper();
        public static GenericCommentManager GenericCommentManager { get; } = new GenericCommentManager();
        public static DocumentationBuilder DocumentationBuilder => new DocumentationBuilder();

    }
}
