using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class MethodTester
    {
        /// <summary>
        /// Shipping address dictionary of list of list.
        /// </summary>
        /// <returns>A string</returns>
        public string ShippingAddressDictionaryOfListOfList()
        {
            return new Dictionary<int, List<List<string>>>();
        }
    }
}
