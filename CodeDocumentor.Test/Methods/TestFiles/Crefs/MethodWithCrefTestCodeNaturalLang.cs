using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp40
{
    public class MethodTester
    {
        public Task<MethodTester> GetAsync(string name)
        {
            return Task.FromResult(new MethodTester());
        }
    }
}
