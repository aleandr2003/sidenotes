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

namespace SideNotes.OAuth
{
    public class VkontakteClient
    {
        public int UserId { get; set; }
        public string AccessToken { get; set; }
        private IConsumerTokenManager _tokenManager;

        public VkontakteClient(IConsumerTokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public string StartAuthentication(string callBackUrl, string scope)
        {
            return String.Format(
             "http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope={1}&redirect_uri={2}&response_type=code",
             _tokenManager.ConsumerKey,
             scope,
             callBackUrl
             );
        }

        public bool FinishAuthentication(string code)
        {
            var query =
                String.Format(
                    "https://api.vkontakte.ru/oauth/access_token?client_id={0}&client_secret={1}&code={2}",
                    _tokenManager.ConsumerKey,
                    _tokenManager.ConsumerSecret,
                    code);

            var req =
                (HttpWebRequest)WebRequest.Create(query);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new InvalidOperationException("Error in reading response");
            var content = new StreamReader(responseStream).ReadToEnd();
            JObject json = JObject.Parse(content);
            JToken errorJToken;
            if (json.TryGetValue("error", out errorJToken))
            {
                var error = errorJToken.Value<string>();
                var errorDescription = json["error_description"].Value<string>();
                throw new InvalidOperationException(String.Format("Error:{0}<br />Error description:{1}", error, errorDescription));
            }

            JToken accessTokenJToken, userIdJToken;
            if (!json.TryGetValue("access_token", out accessTokenJToken) || !json.TryGetValue("user_id", out userIdJToken))
                throw new InvalidOperationException("Bad response");
            AccessToken = accessTokenJToken.Value<string>();
            UserId = userIdJToken.Value<int>();
            return true;
        }

        public string MakePost(string text)
        {
            //var parameters = new Dictionary<string, string>();
            //parameters["api_id"] = _tokenManager.ConsumerKey;
            //parameters["format"] = "json";
            //parameters["method"] = "wall.post";
            //parameters["message"] = text;

            var query = String.Format("https://api.vkontakte.ru/method/wall.post?access_token={0}&message={1}", AccessToken, text);
            //var query = PrepareSignedRequest("http://api.vkontakte.ru/api.php", parameters);
            var req = (HttpWebRequest)WebRequest.Create(query);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            return new StreamReader(responseStream).ReadToEnd();
        }

        public AccountInfo GetAccountInfo(int userId)
        {
            var info = parseInfo(getInfo(userId));
            info.AccountSource = AccountSource.Vkontakte;
            return info;
        }

        private string getInfo(int Id)
        {
            var requestUrl =
                    String.Format(
                        "https://api.vkontakte.ru/method/getProfiles?uid={0}&access_token={1}&fields=photo_big, screen_name",
                        Id,
                        AccessToken);

            var req =
                (HttpWebRequest)WebRequest.Create(requestUrl);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();

            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            return new StreamReader(responseStream).ReadToEnd();
        }

        private AccountInfo parseInfo(string info)
        {
            var json = JObject.Parse(info);
            JToken firstNameJToken, lastNameJToken, photoJToken, userNameJToken, userIdJToken;
            JToken responseJToken = json["response"].Children().First();
            userIdJToken = responseJToken.SelectToken("uid", false);
            firstNameJToken = responseJToken.SelectToken("first_name", false);
            lastNameJToken = responseJToken.SelectToken("last_name", false);
            photoJToken = responseJToken.SelectToken("photo_big", false);
            userNameJToken = responseJToken.SelectToken("screen_name", false);
            if (firstNameJToken == null || lastNameJToken == null)
                throw new InvalidDataException("Bad response");

            var accountInfo = new AccountInfo();
            accountInfo.Name = firstNameJToken.Value<string>() + " " + lastNameJToken.Value<string>();
            accountInfo.Id = userIdJToken.Value<string>();
            accountInfo.UserName = userNameJToken != null ? userNameJToken.Value<string>() : "id" + accountInfo.Id;
            accountInfo.ProfileUrl = String.Format("http://vkontakte.ru/{0}", accountInfo.UserName);
            if (photoJToken != null)
            {
                accountInfo.Photo = photoJToken.Value<string>();
            }
            return accountInfo;
        }

        public List<string> GetFriendIds(int userId)
        {
            var parameters = new Dictionary<string, string>();
            parameters["api_id"] = _tokenManager.ConsumerKey;
            parameters["format"] = "json";
            parameters["method"] = "friends.get";
            parameters["uid"] = userId.ToString();
            var friendsQuery = PrepareSignedRequest("http://api.vkontakte.ru/api.php", parameters);
            var req = (HttpWebRequest)WebRequest.Create(friendsQuery);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            JObject jObject = JObject.Parse(new StreamReader(responseStream).ReadToEnd());
            JToken errorJToken;
            if (jObject.TryGetValue("error", out errorJToken))
            {
                var error = errorJToken.Value<string>();
                JToken descriptionToken = null;
                string description = null;
                if (jObject.TryGetValue("error_description", out descriptionToken))
                    description = descriptionToken.Value<string>();
                if (String.IsNullOrEmpty(description)) throw new InvalidOperationException(String.Format("Error:{0}", error));
                else throw new InvalidOperationException(String.Format("Error:{0}<br />Error description:{1}", error, description));
            }

            return ((JArray)jObject["response"]).Select(t =>
            {
                JToken uid;
                ((JObject)t).TryGetValue("uid", out uid);
                return uid.Value<string>();
            }).ToList();
        }

        private string PrepareSignedRequest(string url, Dictionary<string, string> parameters)
        {
            var paramSorted = parameters.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + "=" + kvp.Value);
            var query = String.Join("&", paramSorted);
            var signature_source = String.Join("", paramSorted);
            signature_source += _tokenManager.ConsumerSecret;
            var signature = signature_source.MD5();
            return url + "?" + query + "&sig=" + signature;
        }

    }
}