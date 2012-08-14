using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using SideNotes.Models;
using System.IO;
using SideNotes.OAuth;
using System.Configuration;
using SideNotes.Services.Abstract;
using SideNotes.Extensions;
using Newtonsoft.Json.Linq;
using SideNotes.Services;
using SideNotes.Models.Queries;

namespace SideNotes.Controllers
{
    public class VkontakteLoginController : OAuthLoginController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;

        public VkontakteLoginController(IUserSession userSession, UserAvatarService avatarService)
            : base(userSession, avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        protected override InMemoryTokenManager GetTokenManager()
        {
            return new VkontakteTokenManagerFactory().GetTokenManager();
        }

        [HttpPost]
        public override ActionResult SendRequest(Uri callbackUri)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1}", Request.QueryString["error"], Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
            var client = new VkontakteClient(GetTokenManager());
            return Redirect(client.StartAuthentication(callbackUrl, "notify, friends"));//, wall, offline
        }

        [HttpPost]
        public ActionResult PostComment(Uri callbackUri, string commentText, int EntityId, int EntityType, string isPrivate)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1}", Request.QueryString["error"], Request.QueryString["error_description"]));

            var manager = new CommentManager(null);
            var tempId = manager.SaveTemporaryComment(EntityId, EntityType, commentText, isPrivate == isChecked);
            //если ещё не аутентифицировались
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()) + "&tempId=" + tempId);
            var client = new VkontakteClient(GetTokenManager());
            return Redirect(client.StartAuthentication(callbackUrl, "notify, friends"));//, wall, offline
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public override ActionResult Callback(Uri callbackUri, int? tempId)
        {
            var code = Request.QueryString["code"];
            if (String.IsNullOrEmpty(code)) throw new ArgumentException("Код авторизации не получен");
            //если уже есть код аутентификации, осталось только получить access_token
            
            var client = new VkontakteClient(GetTokenManager());
            AccountInfo accountInfo;
            try
            {
                client.FinishAuthentication(code);
                accountInfo = client.GetAccountInfo(client.UserId);
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
                        Login = "vk_" + (String.IsNullOrEmpty(accountInfo.UserName) ? accountInfo.Id: accountInfo.UserName)
                    };
                    user.SetForeignAccountId(accountInfo);
                    user.AccountSource = accountInfo.AccountSource;
                    user.VkontakteUsername = accountInfo.UserName;
                    user.VkontakteProfileUrl = accountInfo.ProfileUrl;

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
                user.AccessTokenVkontakte = client.AccessToken;
                user.LastLoginSource = AccountSource.Vkontakte;
                context.SaveChanges();
                userSession.LogIn(user);
                userSession.Sharer = new VkontakteSharer(client.AccessToken, user);
            }
            if (tempId != null)
            {
                var manager = new CommentManager(userSession.Sharer);
                manager.PublishTemporaryComment(tempId ?? 0, userSession.CurrentUser.Id);
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
                    Content(String.Format("{0} {1}", Request.QueryString["error"], Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("AccountProved") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
            var client = new VkontakteClient(GetTokenManager());
            return Redirect(client.StartAuthentication(callbackUrl, "notify, friends, wall, offline"));
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult AccountProved(Uri callbackUri)
        {
            if (!userSession.IsAuthenticated)
                throw new InvalidOperationException("Только зарегистрированный пользователь может добавить профиль в соц. сети");
            var code = Request.QueryString["code"];
            //если уже есть код аутентификации, осталось только получить access_token
            if (!String.IsNullOrEmpty(code))
            {
                var client = new VkontakteClient(GetTokenManager());
                AccountInfo accountInfo;
                try
                {
                    client.FinishAuthentication(code);
                    accountInfo = client.GetAccountInfo(client.UserId);
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
                    if (context.Users.Any(u => u.VkontakteId == accountInfo.Id))
                        return Content("Этот аккаунт vkontakte принадлежит другому пользователю");

                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);

                    user.SetForeignAccountId(accountInfo);
                    user.VkontakteUsername = accountInfo.UserName;
                    user.VkontakteProfileUrl = accountInfo.ProfileUrl;

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
                    
                    user.AccessTokenVkontakte = client.AccessToken;
                    context.SaveChanges();
                }
            }
            return Redirect(callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", new { Id = userSession.CurrentUser.Id }));
        }

        private static void TryAddFriends(VkontakteClient client, SideNotesEntities context, User user)
        {
            List<string> friendIds = null;
            try
            {
                friendIds = client.GetFriendIds(client.UserId);
            }
            catch { }
            if (friendIds != null)
            {
                var friends = context.Users.Where(u => friendIds.Contains(u.VkontakteId)).ToList();
                friends.ForEach(f => user.Friends.Add(f));
            }
        }

        
    }
}
