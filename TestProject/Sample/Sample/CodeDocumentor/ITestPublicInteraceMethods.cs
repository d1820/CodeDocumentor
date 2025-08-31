using System.Threading.Tasks;

namespace Sample.CodeDocumentor
{

  public interface ITestPublicInteraceMethods
    {
        Task<string> GetNamesAsync(string name);
    }
}
