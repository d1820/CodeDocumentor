using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class IndexerTester
    {
        private int[] _data = new int[10];
        public int this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }
    }
}
