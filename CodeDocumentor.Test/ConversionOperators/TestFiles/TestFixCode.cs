using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class ConversionTester
    {
        /// <summary>
        /// Performs an implicit conversion to <see cref="int"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator int(ConversionTester value) => 0;
    }
}
