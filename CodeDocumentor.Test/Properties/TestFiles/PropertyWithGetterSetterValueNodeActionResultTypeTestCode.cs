using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class PropertyTester
    {
        public ActionResult<string> PersonName { get; set; }
    }
    public class ActionResult<T> { }
}
