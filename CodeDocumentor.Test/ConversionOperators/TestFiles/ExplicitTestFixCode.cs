using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class ConversionTester
    {
        /// <summary>
        /// Performs an explicit conversion to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public static explicit operator int(ConversionTester value)
        {
            return 0;
        }
    }
}
