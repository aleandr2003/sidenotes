using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.OAuth;
using System.Configuration;
using SideNotes.Models;
using SideNotes.Models.Queries;
using System.IO;
using System.Net;
using SideNotes.Extensions;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using SideNotes.Controllers.Abstract;


namespace SideNotes.Controllers
{
    public class YandexLoginController : SidenotesController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;

        public YandexLoginController(IUserSession userSession, UserAvatarService avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        protected InMemoryTokenManager GetTokenManager()
        {
            return new YandexTokenManagerFactory().GetTokenManager();
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult SendRequest(Uri callbackUri)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1} {2}", Request.QueryString["error_reason"], Request.QueryString["error"],
                                          Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var client = new YandexClient(GetTokenManager());
            return Redirect(client.StartAuthentication(Uri.EscapeDataString(callbackUri.ToString())));
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult Callback()
        {
            string state = Request.QueryString["state"];
            
            if (state.StartsWith("provingaccount"))
            {
                string callbackUrl = state.Substring("provingaccount".Length);
                return AccountProved(new Uri(callbackUrl));
            }
            else
            {
                return CreateUserAndLogin(new Uri(state));
            }
        }

        public ActionResult CreateUserAndLogin(Uri callbackUri)
        {
            string code = Request.QueryString["code"];
            if (String.IsNullOrEmpty(code)) throw new ArgumentException("Код авторизации не получен");

            var client = new YandexClient(GetTokenManager());
            AccountInfo accountInfo;
            try
            {
                client.FinishAuthentication(code);
                accountInfo = client.GetAccountInfo();
            }
            catch (InvalidOperationException ex)
            {
                return Content(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return Content(ex.Message);
            }

            using (var context = new SideNotesEntities())
            {
                User user = new GetForeignAccountQuery(accountInfo).Load(context);
                if (user == null)
                {
                    user = new User
                    {
                        Name = accountInfo.Name,
                        Login = "ya_" + accountInfo.UserName
                    };
                    user.SetForeignAccountId(accountInfo);
                    user.AccountSource = accountInfo.AccountSource;
                    user.Email = accountInfo.Email;
                    user.YandexUsername = accountInfo.Name;
                    user.YandexProfileUrl = accountInfo.ProfileUrl;

                    context.Users.AddObject(user);
                    context.SaveChanges();

                    Stream imageStream = accountInfo.GetAvatarInputStream();
                    if (imageStream != null)
                    {
                        avatarService.UploadNew(user.Id, imageStream);
                        imageStream.Close();
                    }

                    TryAddFriends(client, context, user);
                }
                else
                {
                    user.SetForeignAccountId(accountInfo);
                }
                user.AccessTokenYandex = client.AccessToken;
                user.LastLoginSource = AccountSource.Yandex;
                context.SaveChanges();
                userSession.LogIn(user);
                userSession.Sharer = new YandexSharer(client.AccessToken, user);
            }
            
            var returnUrl = callbackUri != null ? callbackUri.ToString() : Url.Action("Index", "Home");
            //return Redirect(returnUrl);
            return RedirectToAction("CompleteExternalRegistration", "User", new { callbackUri = returnUrl });
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult ProveAccount(Uri callbackUri)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1} {2}", Request.QueryString["error_reason"], Request.QueryString["error"],
                                          Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var client = new YandexClient(GetTokenManager());
            return Redirect(client.StartAuthentication("provingaccount"+Uri.EscapeDataString(callbackUri.ToString())));
        }

        public ActionResult AccountProved(Uri callbackUri)
        {
            if (!userSession.IsAuthenticated)
                throw new InvalidOperationException("Только зарегистрированный пользователь может добавить профиль в соц. сети");
            string code = Request.QueryString["code"];
            if (!String.IsNullOrEmpty(code))
            {
                var client = new YandexClient(GetTokenManager());
                AccountInfo accountInfo;
                try
                {
                    client.FinishAuthentication(code);
                    accountInfo = client.GetAccountInfo();
                }
                catch (InvalidOperationException ex)
                {
                    return Content(ex.Message);
                }
                catch (InvalidDataException ex)
                {
                    return Content(ex.Message);
                }

                using (var context = new SideNotesEntities())
                {
                    if(context.Users.Any(u => u.YandexId == accountInfo.Id))
                        return Content("Этот yandex-аккаунт принадлежит другому пользователю");

                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                    user.SetForeignAccountId(accountInfo);
                    if (user.Email == null) user.Email = accountInfo.Email;
                    user.YandexUsername = accountInfo.Name;
                    user.YandexProfileUrl = accountInfo.ProfileUrl;
                    
                    if (user.Avatar_Id == null)
                    {
                        Stream imageStream = accountInfo.GetAvatarInputStream();
                        if (imageStream != null)
                        {
                            avatarService.UploadNew(user.Id, imageStream);
                            imageStream.Close();
                        }
                    }
                    TryAddFriends(client, context, user);
                    user.AccessTokenYandex = client.AccessToken;
                    context.SaveChanges();
                }
            }
            return Redirect(callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", new { Id = userSession.CurrentUser.Id }));
        }

        private static void TryAddFriends(YandexClient client, SideNotesEntities context, User user)
        {
            List<string> friendIds = null;
            try
            {
                friendIds = client.GetFriendIds(user.YandexId);
            }
            catch { }

            if (friendIds != null)
            {
                var friends = context.Users.Where(u => friendIds.Contains(u.YandexId)).ToList();
                friends.ForEach(f => user.Friends.Add(f));
            }
        }
    }
}
