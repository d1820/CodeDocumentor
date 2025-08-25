using CodeDocumentor.Helper;
using CodeDocumentor.Managers;

namespace CodeDocumentor.Locators
{
    internal static class ServiceLocator
    {
        public static DocumentationHeaderHelper DocumentationHeaderHelper { get; } = new DocumentationHeaderHelper();
        public static Logger Logger { get; } = new Logger();
        public static CommentHelper CommentHelper { get; } = new CommentHelper();
        public static GenericCommentManager GenericCommentManager { get; } = new GenericCommentManager();

    }
}
