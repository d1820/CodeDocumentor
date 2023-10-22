using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    interface IInterface<TEntity>
    {
        string DoWork();
    }

    public class TestTypes<T>
    {

    }

    public class InhertitTestTypes: TestTypes<int>
    {

    }
    /// <summary>
    /// The field OCR test single class.
    /// </summary>
    internal class FieldOCRTestSingleClass: IInterface<string>
    {
        private static string _testField;
        public int TestProperty { get; set; }

        public FieldOCRTestSingleClass()
        {
                
        }

        public string DoWork()
        {
            return "";
        }

        internal string DoWorkWithParams(string test, string we)
        {
            return "";
        }
        /// <summary>
        /// Work with types.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="test">The test.</param>
        /// <param name="we">The we.</param>
        /// <remarks>
        /// test
        /// </remarks>
        /// <example>
        /// test
        /// </example>
        /// <returns>A string.</returns>
        internal string WorkWithTypes<TEntity>(string test, string we)
        {
            return "";
        }

    }

    internal record TestRecord
    {
        private static string _testField;
        public int TestProperty { get; set; }
    }
}
