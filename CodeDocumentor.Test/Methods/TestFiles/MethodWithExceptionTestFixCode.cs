using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class MethodTester
    {
        /// <summary>
        /// Show method with string return tester.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>A string</returns>
        public string ShowMethodWithStringReturnTester()
        {
            throw new ArgumentNullException("test");
        }
    }
}
