using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth.ChannelElements;
using SideNotes.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SideNotes.OAuth
{
    public class FacebookClient
    {
        public string AccessToken { get; set; }

        IConsumerTokenManager _tokenManager;

        public FacebookClient(IConsumerTokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public string StartAuthentication(string callBackUrl, string scope)
        {
            return StartAuthentication(callBackUrl, scope, null);
        }

        public string StartAuthentication(string callBackUrl, string scope, string state)
        {
            var url = String.Format(
            "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}",
            _tokenManager.ConsumerKey,
            callBackUrl,
            scope
            );
            if (!String.IsNullOrEmpty(state))
            {
                url += "&state=" + state;
            }
            return url;
        }

        public bool FinishAuthentication(string code, string callbackUrl)
        {
            var second =
                String.Format(
                    "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}",
                    _tokenManager.ConsumerKey,
                    callbackUrl,
                    _tokenManager.ConsumerSecret,
                    code);

            var req = (HttpWebRequest)WebRequest.Create(second);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new InvalidOperationException("Error in reading response");
            var responseCollection = HttpUtility.ParseQueryString(new StreamReader(responseStream).ReadToEnd());
            AccessToken = responseCollection.Get("access_token");
            return true;
        }

        public void MakePost(string data)
        {
            string content = "access_token=" + AccessToken + "&" + data;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            var byteArray = encoding.GetBytes(content);
            var req = (HttpWebRequest)WebRequest.Create("https://graph.facebook.com/me/feed");
            req.Method = "POST";
            req.ContentLength = byteArray.Length;
            Stream dataStream = req.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            // return (new StreamReader(responseStream)).ReadToEnd();
        }

        public AccountInfo GetAccountInfo()
        {
            var info = parseInfo(getInfo());
            info.AccountSource = AccountSource.Facebook;
            return info;
        }

        private string getInfo()
        {
            var query = String.Format("https://graph.facebook.com/me?access_token={0}", AccessToken);
            var req = (HttpWebRequest)WebRequest.Create(query);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            return new StreamReader(responseStream).ReadToEnd();
        }

        private AccountInfo parseInfo(string info)
        {
            JObject json = JObject.Parse(info);
            JToken nameJToken, emailJToken, userNameJtoken, uidJToken, linkJToken;
            nameJToken = json.SelectToken("name", false);
            emailJToken = json.SelectToken("email", false);
            userNameJtoken = json.SelectToken("username", false);
            uidJToken = json.SelectToken("id", false);
            linkJToken = json.SelectToken("link", false);
            if (nameJToken == null || uidJToken == null) throw new InvalidDataException("Bad response");

            var accountInfo = new AccountInfo();
            accountInfo.Name = nameJToken.Value<string>();
            accountInfo.Id = uidJToken.Value<string>();
            if (emailJToken != null) accountInfo.Email = emailJToken.Value<string>();
            accountInfo.UserName = userNameJtoken != null ? userNameJtoken.Value<string>() : accountInfo.Name;
            accountInfo.Photo = String.Format("http://graph.facebook.com/{0}/picture?type=large", accountInfo.Id);
            if (linkJToken != null) accountInfo.ProfileUrl = linkJToken.Value<string>();
            return accountInfo;
        }

        public List<string> GetFriendIds()
        {
            string query = "SELECT uid2 FROM friend WHERE uid1 = me()";
            var friendsQuery = String.Format("https://api.facebook.com/method/fql.query?access_token={0}&query={1}&format=json", AccessToken, query);
            var req = (HttpWebRequest)WebRequest.Create(friendsQuery);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            JArray jArray = JArray.Parse(new StreamReader(responseStream).ReadToEnd());
            return jArray.Select(t =>
            {
                JToken uid2;
                ((JObject)t).TryGetValue("uid2", out uid2);
                return uid2.Value<string>();
            }).ToList();
        }
        
    }
}