
using CodeDocumentor.Common.Interfaces;

namespace CodeDocumentor.Analyzers.Services
{
    public class PreLoadLogger : IEventLogger
    {
        public void LogDebug(string category, string message)
        {
        }

        public void LogError(string message, int eventId, short category, string diagnosticId)
        {

        }

        public void LogInfo(string message, int eventId, short category, string diagnosticId)
        {

        }
    }
}
