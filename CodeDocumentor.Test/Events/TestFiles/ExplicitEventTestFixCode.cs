using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public class EventTester
    {
        private EventHandler _handler;

        /// <summary>
        /// Occurs when button clicked.
        /// </summary>
        public event EventHandler ButtonClicked
        {
            add { _handler += value; }
            remove { _handler -= value; }
        }
    }
}
