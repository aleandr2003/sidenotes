using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Text;

namespace SideNotes.OAuth
{
    public class LivejournalClient
    {
        private XDocument foafData;
        private string nickName;
        private XmlNamespaceManager nsManager;
        private string foafUrl;

        public LivejournalClient(string url)
        {
            this.nickName = GetNickFromUrl(url);
            foafUrl = String.Format("http://{0}.livejournal.com/data/foaf", this.nickName);
            nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            nsManager.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
            nsManager.AddNamespace("foaf", "http://xmlns.com/foaf/0.1/");
        }

        public static string GetNickFromUrl(string url)
        {
            int startindex = 0;
            int lastindex = 0;
            if (url.StartsWith("http://")) startindex = 7;
            else if (url.StartsWith("https://")) startindex = 8;
            lastindex = url.IndexOf(".livejournal.com/");

            int length = lastindex - startindex;
            return url.Substring(startindex, length);
        }

        private void getFoafDocument()
        {
            var req = (HttpWebRequest)WebRequest.Create(foafUrl);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            foafData = XDocument.Parse(new StreamReader(responseStream).ReadToEnd());
        }

        public AccountInfo GetAccountInfo()
        {
            if (foafData == null) getFoafDocument();
            XElement personNode = foafData.XPathSelectElement("/rdf:RDF/foaf:Person", nsManager);
            XElement FullName = personNode.XPathSelectElement("foaf:name", nsManager);

            string Id = null, Photo = null;
            XElement imgElement = personNode.XPathSelectElement("foaf:img", nsManager);
            if (imgElement != null)
            {
                IEnumerable collection = (IEnumerable)imgElement.XPathEvaluate("@rdf:resource[1]", nsManager);
                XAttribute imgAttr = collection.Cast<XAttribute>().FirstOrDefault();

                Id = imgAttr.Value.Substring(imgAttr.Value.LastIndexOf("/") + 1);
                Photo = imgAttr.Value;
            }
            else
            {
                Id = getUserId();
            }

            var info = new AccountInfo();
            info.Name = FullName.Value;
            info.UserName = nickName;
            info.Photo = Photo;
            info.Id = Id;
            info.ProfileUrl = String.Format("http://{0}.livejournal.com/profile", info.UserName);

            info.AccountSource = AccountSource.Livejournal;
            return info;
        }

        public List<string> GetFriendIds()
        {
            if (foafData == null) getFoafDocument();
            XElement personNode = foafData.XPathSelectElement("/rdf:RDF/foaf:Person", nsManager);
            var elements = personNode.XPathSelectElements("foaf:knows/foaf:Person/foaf:image", nsManager);
            return elements.Select(e => e.Value.Substring(e.Value.LastIndexOf("/") + 1)).ToList();
        }

        private string getUserId()
        {
            var foafQuery = String.Format("http://{0}.livejournal.com/profile", nickName);
            var req = (HttpWebRequest)WebRequest.Create(foafQuery);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");

            int bufferLength = 2000;
            StringBuilder doc = new StringBuilder(bufferLength);
            StreamReader reader = new StreamReader(responseStream);
            char[] buffer = new char[bufferLength];
            while (!doc.ToString().Contains("\"userid\":"))
            {
                reader.ReadBlock(buffer, 0, bufferLength);
                doc.Append(buffer);
            }
            reader.ReadBlock(buffer, 0, 200);
            doc.Append(buffer);
            string docstr = doc.ToString();
            string id = docstr.Substring(docstr.LastIndexOf("\"userid\":\"") + 10);
            id = id.Substring(0, id.IndexOf("\""));
            return id;
        }
    }
}