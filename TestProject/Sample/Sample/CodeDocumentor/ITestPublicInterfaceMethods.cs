using System.Threading.Tasks;

namespace Sample.CodeDocumentor
{

  public interface ITestPublicInterfaceMethods
  {

    Task<string> GetNamesAsync(string name, string age);
  }
}
