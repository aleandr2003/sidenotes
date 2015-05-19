using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OAuth.ChannelElements;
using SideNotes.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using SideNotes.Extensions;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

namespace SideNotes.OAuth
{
    public class YandexClient
    {
        public string AccessToken { get; set; }
        private IConsumerTokenManager _tokenManager;
        private XmlNamespaceManager nsManager;
        private string postsUrlTemplate = "https://api-yaru.yandex.ru/person/{0}/post/";

        public YandexClient(IConsumerTokenManager tokenManager)
        {
            _tokenManager = tokenManager;

            nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("ya", "http://api.yandex.ru/yaru/");
        }

        public string StartAuthentication(string state)
        {
            return String.Format(
            "https://oauth.yandex.ru/authorize?response_type=code&client_id={0}&state={1}",
            _tokenManager.ConsumerKey,
            state
            );
        }

        public bool FinishAuthentication(string code)
        {
            var query = "https://oauth.yandex.ru/token";

            var data = String.Format("grant_type=authorization_code&code={0}&client_id={1}&client_secret={2}",
                code,
                _tokenManager.ConsumerKey,
                _tokenManager.ConsumerSecret);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            var byteArray = encoding.GetBytes(data);

            var req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "POST";
            req.ContentLength = byteArray.Length;
            req.ContentType = "application/x-www-form-urlencoded";
            Stream dataStream = req.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new InvalidOperationException("Error in reading response");

            //var responseCollection = HttpUtility.ParseQueryString(new StreamReader(responseStream).ReadToEnd());
            var jResponse = JObject.Parse(new StreamReader(responseStream).ReadToEnd());
            JToken jError;
            if (jResponse.TryGetValue("error", out jError))
            {
                string errorMessage = "";
                if (jError.ToString() == "invalid_request") errorMessage = "неверный формат запроса";
                else if (jError.ToString() == "invalid_grant") errorMessage = "неверный или просроченный код подтверждения";
                else if (jError.ToString() == "unsupported_grant_type") errorMessage = "неверное значение параметра grant_type";
                else errorMessage = "неизвестная ошибка";
                throw new InvalidOperationException(errorMessage);
            }
            JToken authToken = jResponse["access_token"];
            AccessToken = authToken.ToString();
            return true;
        }

        public void MakePost(string data, string userId)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            var byteArray = encoding.GetBytes(data);
            var req = (HttpWebRequest)WebRequest.Create(String.Format(postsUrlTemplate, userId));
            req.Headers.Add("Authorization", "OAuth " + AccessToken);
            req.Method = "POST";
            req.ContentLength = byteArray.Length;
            req.ContentType = "application/atom+xml; type=entry";
            Stream dataStream = req.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = req.GetResponse();
            // var responseStream = response.GetResponseStream();
            // return (new StreamReader(responseStream)).ReadToEnd();
        }

        public AccountInfo GetAccountInfo()
        {
            var info = parseInfo(getInfo());
            info.AccountSource = AccountSource.Yandex;
            return info;
        }

        private string getInfo()
        {
            if (String.IsNullOrEmpty(AccessToken)) return null;
            var req = (HttpWebRequest)WebRequest.Create("https://api-yaru.yandex.ru/me/");
            req.Headers.Add("Authorization", "OAuth " + AccessToken);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            return new StreamReader(responseStream).ReadToEnd();
        }

        private AccountInfo parseInfo(string info)
        {
            XDocument doc = XDocument.Parse(info);

            var accountInfo = new AccountInfo();
            XElement IdNode = doc.XPathSelectElement("/ya:person/ya:id", nsManager);
            if (IdNode != null) accountInfo.Id = IdNode.Value.Substring(IdNode.Value.IndexOf("person/") + 7);

            XElement NameNode = doc.XPathSelectElement("/ya:person/ya:name", nsManager);
            if (NameNode != null) accountInfo.Name = NameNode.Value;

            XElement EmailNode = doc.XPathSelectElement("/ya:person/ya:email", nsManager);
            if (EmailNode != null) accountInfo.Email = EmailNode.Value;

            XElement PhotoNode = doc.XPathSelectElement("/ya:person/ya:link[@rel='userpic']", nsManager);
            if (PhotoNode != null) accountInfo.Photo = PhotoNode.Attribute("href").Value;

            XElement SelfNode = doc.XPathSelectElement("/ya:person/ya:link[@rel='www']", nsManager);
            if (SelfNode != null) accountInfo.ProfileUrl = SelfNode.Attribute("href").Value;

            accountInfo.UserName = accountInfo.ProfileUrl.Substring("http://".Length);
            accountInfo.UserName = accountInfo.UserName.Substring(0, accountInfo.UserName.IndexOf(".ya.ru"));

            return accountInfo;
        }

        public List<string> GetFriendIds(string userId)
        {
            if (String.IsNullOrEmpty(AccessToken)) return null;
            var query = String.Format("https://api-yaru.yandex.ru/person/{0}/friend/", userId);
            var req = (HttpWebRequest)WebRequest.Create(query);
            req.Headers.Add("Authorization", "OAuth " + AccessToken);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            var text = new StreamReader(responseStream).ReadToEnd();
            XDocument doc = XDocument.Parse(text);
            var IdNodes = doc.XPathSelectElements("/ya:persons/ya:person/ya:id", nsManager);
            return IdNodes.Select(n => n.Value.Substring(n.Value.IndexOf("person/") + 7)).ToList();
        }
    }
}