using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public interface IInterfaceTesterPrivateMethod
    {
        /// <summary>
        ///  Gets the names asynchronously.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<string> GetNamesAsync(string name);
    }
}
