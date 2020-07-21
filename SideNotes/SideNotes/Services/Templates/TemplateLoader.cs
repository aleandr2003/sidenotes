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
        private Dictionary<string, EmailTemplate> emailTemplatesCache = new Dictionary<string, EmailTemplate>();
        private object cacheLock = new object();

        public TemplateLoader(string path)
        {
            this.path = path;
            if (!this.path.EndsWith("\\"))
            {
                this.path += "\\";
            }

            
        }

        public EmailTemplate GetEmailTemplate(string templateName, string culture = "ru")
        {
            string cacheKey = GetCacheKey(templateName, culture);
            lock (cacheLock)
            {
                if (emailTemplatesCache.ContainsKey(cacheKey))
                {
                    return emailTemplatesCache[cacheKey];
                }
                EmailTemplate template = LoadEmailTemplate(templateName, culture);
                emailTemplatesCache[cacheKey] = template;
                return template;
            }
        }

        private EmailTemplate LoadEmailTemplate(string templateName, string culture = "ru")
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

        private string GetCacheKey(string templateName, string culture) => $"{templateName}.{culture}";

        private string GetFileName(string templateName, string culture)
        {
            string filename;
            if (this.path.StartsWith(".\\") || this.path.StartsWith("~\\"))
            {
                filename = $"{path}{templateName}";
            }
            else
            {
                string appRoot = HttpRuntime.AppDomainAppPath;
                filename = $"{appRoot}{path}{templateName}";
            }
            
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