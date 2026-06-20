using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class ConversionTester
    {
        public static implicit operator int(ConversionTester value)
        {
            return 0;
        }
    }
}
