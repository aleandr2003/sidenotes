using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.TestClasses
{
    public class WoodenDummy :IDummy
    {
        public string GetMessage()
        {
            return "Wooden dummy";
        }
    }
}