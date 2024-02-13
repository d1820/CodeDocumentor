using CodeDocumentor.Helper;
using CodeDocumentor.Managers;
using CodeDocumentor.Services;
using CodeDocumentor.Vsix2022;
using SimpleInjector;

namespace CodeDocumentor
{
    public static class ApplicationRegistrations
    {
        public static void RegisterServices(this Container container)
        {
            container.RegisterSingleton<IOptionsService>(() =>
            {
                var opts = new OptionsService();
                opts.SetDefaults(CodeDocumentorPackage.Options);
                return opts;
            });
            //NOTE keep these in sync with unit test container
            container.RegisterSingleton<GenericCommentManager>();
            container.Register<DocumentationBuilder>();
        }
    }
}
