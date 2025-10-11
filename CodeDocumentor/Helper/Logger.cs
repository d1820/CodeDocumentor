

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System.Diagnostics;
using CodeDocumentor.Common.Interfaces;

namespace CodeDocumentor
{
    public class Logger : IEventLogger
    {
        /// <summary>
        ///  Logs the error.
        /// </summary>
        /// <param name="message"> The message. </param>
        public void LogError(string message, int eventId, short category, string diagnosticId)
        {
            try
            {
                // I'm co-opting the Visual Studio event source because I can't register my own from a .VSIX installer.
                EventLog.WriteEntry("Visual Studio",
                    $"CodeDocumentor: DiagnosticId: {diagnosticId}. Message: {message ?? "null"}",
                    EventLogEntryType.Error, eventId, category);
            }
            catch
            {
                // Don't kill extension for logging errors
            }
        }

        public void LogInfo(string message, int eventId, short category, string diagnosticId)
        {
            try
            {
                // I'm co-opting the Visual Studio event source because I can't register my own from a .VSIX installer.
                EventLog.WriteEntry("Visual Studio",
                    $"CodeDocumentor: DiagnosticId: {diagnosticId}. Message: {message ?? "null"}",
                    EventLogEntryType.Information, eventId, category);
            }
            catch
            {
                // Don't kill extension for logging errors
            }
        }
    }
}
