using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.TestClasses
{
    public class PlasticDummy :IDummy
    {
        public string GetMessage()
        {
            return "Plastic dummy";
        }
    }
}