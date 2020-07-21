using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Exceptions
{
    public class TemplateNotFoundException : SidenotesException
    {
        public TemplateNotFoundException() { }

        public TemplateNotFoundException(string message) : base(message) { }
    }
}