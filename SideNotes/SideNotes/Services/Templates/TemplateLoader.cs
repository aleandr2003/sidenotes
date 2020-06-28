using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SideNotes.Services.Templates
{
    public class TemplateLoader : ITemplateLoader
    {
        private string path;

        public TemplateLoader(string path)
        {
            this.path = path;
            if (!this.path.EndsWith("\\"))
            {
                this.path += "\\";
            }
        }

        public EmailTemplate LoadEmailTemplate(string templateName, string culture = "ru")
        {
            string filename = GetFileName(templateName, culture);
            if (!File.Exists(filename))
            {
                return null;
            }
            string fileContent = File.ReadAllText(filename);

            XDocument doc;
            try
            {
                doc = XDocument.Parse(fileContent);
            }
            catch (Exception ex)
            {
                //TODO log error
                return null;
            }
            string subject = doc.XPathSelectElement("/template/subject")?.Value;
            string body = doc.XPathSelectElement("/template/body")?.Value;
            return new EmailTemplate()
            {
                Subject = subject,
                Body = body
            };
        }

        private string GetFileName(string templateName, string culture)
        {
            string filename = $"{path}{templateName}";
            if (culture.ToLower() == "ru")
            {
                return filename + ".xml";
            }
            else
            {
                return filename + $".{culture}.xml";
            }
        }
    }
}