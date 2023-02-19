using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Test.TestHelpers
{
    [SuppressMessage("XMLDocumentation", "")]
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            Path = path;
            Line = line;
            Column = column;
        }

        public string Path { get; }

        public int Line { get; }

        public int Column { get; }
    }

    [SuppressMessage("XMLDocumentation", "")]
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (locations == null)
                {
                    locations = new DiagnosticResultLocation[] { };
                }
                return locations;
            }

            set
            {
                locations = value;
            }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Path : "";
            }
        }

        public int Line
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Line : -1;
            }
        }

        public int Column
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Column : -1;
            }
        }
    }
}
