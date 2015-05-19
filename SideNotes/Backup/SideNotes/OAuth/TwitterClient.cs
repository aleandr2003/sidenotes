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
    public class TwitterClient
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }

        private static readonly ServiceProviderDescription ServiceDescription =
            new ServiceProviderDescription
            {
                RequestTokenEndpoint = new MessageReceivingEndpoint(
                                           "http://twitter.com/oauth/request_token",
                                           HttpDeliveryMethods.GetRequest |
                                           HttpDeliveryMethods.AuthorizationHeaderRequest),
                UserAuthorizationEndpoint = new MessageReceivingEndpoint(
                                          "http://twitter.com/oauth/authorize",
                                          HttpDeliveryMethods.GetRequest |
                                          HttpDeliveryMethods.AuthorizationHeaderRequest),
                AccessTokenEndpoint = new MessageReceivingEndpoint(
                                          "http://twitter.com/oauth/access_token",
                                          HttpDeliveryMethods.GetRequest |
                                          HttpDeliveryMethods.AuthorizationHeaderRequest),
                TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
            };
        private static readonly MessageReceivingEndpoint TweetEndpoint = new MessageReceivingEndpoint("https://api.twitter.com/1/statuses/update.json", HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);
        IConsumerTokenManager _tokenManager;

        public TwitterClient(IConsumerTokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        public void StartAuthentication(string callBackUrl)
        {
            var request = HttpContext.Current.Request;
            using (var twitter = new WebConsumer(ServiceDescription, _tokenManager))
            {
                twitter.Channel.Send(
                    twitter.PrepareRequestUserAuthorization(new Uri(callBackUrl), null, null)
                );
            }
        }

        public bool FinishAuthentication()
        {
            using (var twitter = new WebConsumer(ServiceDescription, _tokenManager))
            {
                var accessTokenResponse = twitter.ProcessUserAuthorization();
                if (accessTokenResponse != null)
                {
                    UserName = accessTokenResponse.ExtraData["screen_name"];
                    AccessToken = accessTokenResponse.AccessToken;
                    return true;
                }
            }

            return false;
        }

        public string MakeTweet(string text)
        {
            using (var twitter = new WebConsumer(ServiceDescription, _tokenManager))
            {
                var data = new Dictionary<string,string>();
                data.Add("status", text);
                HttpWebRequest request = twitter.PrepareAuthorizedRequest(TweetEndpoint, AccessToken, data);
                IncomingWebResponse response = twitter.Channel.WebRequestHandler.GetResponse(request);
                string responseString = response.GetResponseReader().ReadToEnd();
                return responseString;
            }
        }

        public AccountInfo GetAccountInfo(string screenName)
        {
            var info = parseInfo(getInfo(screenName));
            info.AccountSource = AccountSource.Twitter;
            return info;
        }

        private string getInfo(string screenName)
        {
            var url = String.Format("http://api.twitter.com/1/users/show.json?screen_name={0}", screenName);
            var req = (HttpWebRequest)WebRequest.Create(url);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            return new StreamReader(responseStream).ReadToEnd();
        }

        private AccountInfo parseInfo(string info)
        {
            JObject json = JObject.Parse(info);
            JToken nameJToken, userNameJtoken, uidJToken;
            if (!json.TryGetValue("name", out nameJToken)
                ||
                !json.TryGetValue("screen_name", out userNameJtoken)
                ||
                !json.TryGetValue("id", out uidJToken)
                )
                throw new InvalidDataException("Bad response");
            var accountInfo = new AccountInfo();
            accountInfo.Name = nameJToken.Value<string>();
            accountInfo.Id = uidJToken.Value<string>();
            accountInfo.UserName = userNameJtoken.Value<string>();
            accountInfo.ProfileUrl = String.Format("http://twitter.com/#!/{0}", accountInfo.UserName);
            accountInfo.Photo = String.Format("http://api.twitter.com/1/users/profile_image?screen_name={0}&size=bigger", accountInfo.UserName);
            return accountInfo;
        }

        public List<string> GetFriendIds(string screenName)
        {
            var friendsQuery = String.Format("https://api.twitter.com/1/friends/ids.json?screen_name={0}", screenName);
            var req = (HttpWebRequest)WebRequest.Create(friendsQuery);
            var response = req.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
                throw new InvalidDataException("Error in reading response");
            var responseText = new StreamReader(responseStream).ReadToEnd();
            JArray jArray = null;
            try
            {
                jArray = JArray.Parse(responseText);
            }
            catch { }
            if (jArray == null)
            {
                JObject json = JObject.Parse(responseText);
                jArray = (JArray)json["ids"];
            }
            return jArray.Select(t => t.Value<string>()).ToList();
        }
        
    }
}