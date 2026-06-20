using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class OperatorTester
    {
        public static OperatorTester operator +(OperatorTester a, OperatorTester b)
        {
            return new OperatorTester();
        }
    }
}
