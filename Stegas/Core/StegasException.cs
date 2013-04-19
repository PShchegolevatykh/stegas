using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core
{
    public class StegasException : ApplicationException
    {
        public StegasException() : base()
        {
        }
        public StegasException(string message) : base(message)
        {
        }
        public StegasException(string message, Exception inner): base(message, inner)
        {
        }
    }
}
