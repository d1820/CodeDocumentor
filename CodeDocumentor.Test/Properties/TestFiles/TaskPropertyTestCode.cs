using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class PropertyTester
    {
        public Task<int> ShippingAddressAsync { get; set; }
    }
    public class Task<T> { }
}
