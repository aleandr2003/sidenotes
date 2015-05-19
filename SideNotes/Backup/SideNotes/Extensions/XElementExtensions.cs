using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Text;

namespace SideNotes.Extensions
{
    public static class XElementExtensions
    {
        public static string InnerXml(this XElement element)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var el in element.Nodes()) sb.Append(el.ToString());
            return sb.ToString();
        }
    }
}