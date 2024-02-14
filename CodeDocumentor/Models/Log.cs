using System;
using System.Diagnostics;

// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
namespace CodeDocumentor.Vsix2022
{
    internal static class Log
    {
        /// <summary>
        ///  Logs the error.
        /// </summary>
        /// <param name="message"> The message. </param>
        internal static void LogError(string message, int eventId, short category)
        {
            try
            {
                // I'm co-opting the Visual Studio event source because I can't register my own from a .VSIX installer.
                EventLog.WriteEntry("Visual Studio",
                    "CodeDocumentor: " + (message ?? "null"),
                    EventLogEntryType.Error,eventId, category);
            }
            catch
            {
                // Don't kill extension for logging errors
            }
        }
    }
}
