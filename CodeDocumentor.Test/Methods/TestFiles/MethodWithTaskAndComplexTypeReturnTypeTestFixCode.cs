using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class MethodTester
    {
        /// <summary>
        /// Creates and return a task of type actionresult of type clientdto asynchronously.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <returns><![CDATA[Task<ActionResult<ClientDto>>]]></returns>
        public Task<ActionResult<ClientDto>> CreateAsync()
        {
            throw new ArgumentException("test");
        }
    }
    public class Task<T> { }
    public class ActionResult<T> { }
    public class ClientDto { }
}
