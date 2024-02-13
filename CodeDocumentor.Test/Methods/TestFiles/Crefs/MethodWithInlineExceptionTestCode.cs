using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp40
{
    public class MethodTester
    {
        public string ShowMethodWithStringReturnTester()
        {
            ArgumentNullException.ThrowIfNull("test", "test");
        }
    }
}
