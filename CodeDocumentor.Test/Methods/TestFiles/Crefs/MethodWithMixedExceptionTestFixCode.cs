using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp40
{
    public class MethodTester
    {
        /// <summary>
        /// Show method with string return tester.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>A <see cref="string"/></returns>
        public string ShowMethodWithStringReturnTester()
        {
            ArgumentNullException.ThrowIfNull("test", "test");
            ArgumentException.ThrowIfNullOrEmpty("test", "test");
        }
    }
}
