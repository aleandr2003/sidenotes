using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Exceptions
{
    public class SidenotesException : Exception
    {
        public SidenotesException() { }

        public SidenotesException(string message) : base(message) { }
    }
}