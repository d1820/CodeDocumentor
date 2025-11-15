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
#if DEBUG
                // In debug mode, use Debug.WriteLine to avoid permission issues with Event Log
                Debug.WriteLine($"[CodeDocumentor2026] ERROR - DiagnosticId: {diagnosticId}, EventId: {eventId}, Category: {category}, Message: {message ?? "null"}");
#else
                // In release mode, try to write to Event Log but don't fail if we can't
                try
                {
                    EventLog.WriteEntry("Visual Studio",
                        $"CodeDocumentor2026: DiagnosticId: {diagnosticId}. Message: {message ?? "null"}",
                        EventLogEntryType.Error, eventId, category);
                }
                catch
                {
                    // Fallback to Debug output if Event Log fails
                    Debug.WriteLine($"[CodeDocumentor2026] ERROR - DiagnosticId: {diagnosticId}, EventId: {eventId}, Category: {category}, Message: {message ?? "null"}");
                }
#endif
            }
            catch
            {
                // Don't kill extension for logging errors - silent fallback
            }
        }

        public void LogDebug(string category, string message)
        {
            try
            {
#if DEBUG
                if (!string.IsNullOrEmpty(category))
                {
                    Debug.WriteLine($"[{category.ToUpper()}]: {message}");
                }
                else
                {
                    Debug.WriteLine(message);
                }
#endif
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
#if DEBUG
                // In debug mode, use Debug.WriteLine to avoid permission issues
                Debug.WriteLine($"[CodeDocumentor2026] INFO - DiagnosticId: {diagnosticId}, EventId: {eventId}, Category: {category}, Message: {message ?? "null"}");
#else
                // In release mode, try to write to Event Log but don't fail if we can't
                try
                {
                    EventLog.WriteEntry("Visual Studio",
                        $"CodeDocumentor2026: DiagnosticId: {diagnosticId}. Message: {message ?? "null"}",
                        EventLogEntryType.Information, eventId, category);
                }
                catch
                {
                    // Fallback to Debug output if Event Log fails
                    Debug.WriteLine($"[CodeDocumentor2026] INFO - DiagnosticId: {diagnosticId}, EventId: {eventId}, Category: {category}, Message: {message ?? "null"}");
                }
#endif
            }
            catch
            {
                // Don't kill extension for logging errors - silent fallback
            }
        }
    }
}
