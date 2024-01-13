using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class MethodTester
    {
        /// <summary>
        /// Gets a <see cref="MethodTester"/> asynchronously.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><![CDATA[Task<MethodTester>]]></returns>
        public Task<MethodTester> GetAsync(string name)
        {
            return Task.FromResult(new MethodTester());
        }
    }
}
