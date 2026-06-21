using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    /// <summary>
    /// The transform handler.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="count">The count.</param>
    /// <returns>A string</returns>
    public delegate string TransformHandler(string input, int count);
}
