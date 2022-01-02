using CodeDocumentor.Vsix2022;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeDocumentor.Test
{
    [TestClass]
    public class Startup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Runtime.RunningUnitTests = true;
        }
    }
}
