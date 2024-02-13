using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp40
{
    public class MethodTester
    {
        /// <summary>
        /// Get and return a <see cref="Task"/> of type <see cref="MethodTester"/> asynchronously.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A <see cref="Task"/> of type <see cref="MethodTester"/></returns>
        public Task<MethodTester> GetAsync(string name)
        {
            return Task.FromResult(new MethodTester());
        }
    }
}
