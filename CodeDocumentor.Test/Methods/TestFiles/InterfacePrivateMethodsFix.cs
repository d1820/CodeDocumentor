using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public interface IInterfaceTesterPrivateMethod
    {
        /// <summary>
        /// Get the names asynchronously.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><![CDATA[Task<string>]]></returns>
        Task<string> GetNamesAsync(string name);
    }
}
