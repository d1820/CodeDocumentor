using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class OperatorTester
    {
        /// <summary>
        /// Implements the + operator.
        /// </summary>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <returns>An OperatorTester</returns>
        public static OperatorTester operator +(OperatorTester a, OperatorTester b)
        {
            return new OperatorTester();
        }
    }
}
