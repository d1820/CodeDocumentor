using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class IndexerTester
    {
        private int[] _data = new int[10];
        /// <summary>
        /// Gets or sets the element.
        /// </summary>
        /// <param name="index">The index.</param>
        public int this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }
    }
}
