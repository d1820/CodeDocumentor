using CodeDocumentor.Services;
using SimpleInjector;

namespace CodeDocumentor
{
    public static class ApplicationRegistrations
    {
        public static void RegisterServices(this Container container)
        {
            container.RegisterSingleton<IOptionsService, OptionsService>();
        }
    }
}
