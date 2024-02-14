using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class PropertyTester
    {
        /// <summary>
        /// Gets or sets the shipping address asynchronously.
        /// </summary>
        public Task<int> ShippingAddressAsync { get; set; }
    }
    public class Task<T> { }
}
