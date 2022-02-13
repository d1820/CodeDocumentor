using System;
using System.IO;
using System.Linq;
using CodeDocumentor.Vsix2022;
using FluentAssertions;

namespace CodeDocumentor.Test
{
    public class TestFixure
    {
        public const string DIAG_TYPE_PUBLIC = "public";
        public const string DIAG_TYPE_PRIVATE = "private";

        public TestFixure()
        {
            Runtime.RunningUnitTests = true;
        }
        public string LoadTestFile(string relativePath)
        {
            return File.ReadAllText(relativePath);
        }

        public void AssertOutputContainsCount(string[] source, string searchTerm, int numOfTimes)
        {
            var matchQuery = from word in source
                             where word.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) > -1
                             select word;

            matchQuery.Count().Should().Be(numOfTimes);
        }
    }
}
