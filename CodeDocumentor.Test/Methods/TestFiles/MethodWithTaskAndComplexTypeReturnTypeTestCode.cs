using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class MethodTester
    {
        public Task<ActionResult<ClientDto>> CreateAsync()
        {
            throw new ArgumentException("test");
        }
    }
    public class Task<T> { }
    public class ActionResult<T> { }
    public class ClientDto { }
}
