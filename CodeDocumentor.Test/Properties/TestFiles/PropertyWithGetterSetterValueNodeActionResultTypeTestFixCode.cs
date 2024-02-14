using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class PropertyTester
    {
        /// <summary>
        /// Gets or sets the person name.
        /// </summary>
        /// <value>An <see cref="ActionResult"/> of type <see cref="string"/></value>
        public ActionResult<string> PersonName { get; set; }
    }
    public class ActionResult<T> { }
}
