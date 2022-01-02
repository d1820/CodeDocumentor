using System.Diagnostics.CodeAnalysis;
using CodeDocumentor.Vsix2022;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeDocumentor.Test
{
    [TestClass]
    [SuppressMessage("XMLDocumentation", "")]
    public class Startup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Runtime.RunningUnitTests = true;
        }
    }
}
