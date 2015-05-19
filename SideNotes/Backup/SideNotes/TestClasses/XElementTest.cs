using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using SideNotes.Extensions;

namespace SideNotes.TestClasses
{
    public class XElementTest
    {
        private string str = "представителей народа <a xlink:href=\"#note_1\" type=\"note\">{1}</a>";
        private XmlNamespaceManager nsManager;
        private XNamespace xlink;

        public XElementTest() {
            nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("x", "http://www.gribuser.ru/xml/fictionbook/2.0");
            xlink = "http://www.w3.org/1999/xlink";
        }

        public string Test()
        {
            XElement element = XElement.Parse("<p xmlns:xlink=\"http://www.w3.org/1999/xlink\">"+str+"</p>");
            var links = element.XPathSelectElements("*[@type='note']", nsManager).ToList();
            links.ForEach(l => 
                l.ReplaceWith(
                    XElement.Parse(
                        "<a href=\"javascript:void(0)\" rel=\"" 
                            + 
                            l.Attribute(xlink + "href").Value.Substring(1) 
                            + "\">" + l.Value +"</a>")
                )
            );
            //var ids = links.Select(l => l.Attribute(xlink + "href")).Select(a => a.Value);
            return element.InnerXml();
        }
    }
}