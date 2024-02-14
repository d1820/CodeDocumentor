using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CodeDocumentor.Test.TestHelpers
{
    public class PriorityOrderer : ITestCaseOrderer
    {
        public const string FullName = "CodeDocumentor.Test.TestHelpers.PriorityOrderer";
        public const string AssemblyName = "CodeDocumentor.Test";

        /// <inheritdoc/>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (var testCase in testCases)
            {
                var priority = 0;

                foreach (var attr in testCase.TestMethod.Method.GetCustomAttributes(typeof(PriorityAttribute).AssemblyQualifiedName))
                {
                    priority = attr.GetNamedArgument<int>("Priority");
                }

                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
            {
                //run them in name order after priority order
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
                foreach (var testCase in list)
                {
                    yield return testCase;
                }
            }
        }

        static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }

            result = new TValue();
            dictionary[key] = result;

            return result;
        }
    }
}
