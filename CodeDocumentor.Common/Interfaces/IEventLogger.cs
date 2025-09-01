// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Common.Interfaces
{
    public interface IEventLogger
    {
        void LogError(string message, int eventId, short category, string diagnosticId);
        void LogInfo(string message, int eventId, short category, string diagnosticId);
    }
}
