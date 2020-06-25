using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using System.IO;
using System.Net;
using SideNotes.OAuth;
using SideNotes.Extensions;
using System.Configuration;
using SideNotes.Models;
using Newtonsoft.Json.Linq;
using SideNotes.Models.Queries;
using SideNotes.Services;

namespace SideNotes.Controllers
{
    public class FacebookLoginController : OAuthLoginController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;

        public FacebookLoginController(IUserSession userSession, UserAvatarService avatarService)
            : base(userSession, avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        protected override InMemoryTokenManager GetTokenManager()
        {
            return new FacebookTokenManagerFactory().GetTokenManager();
        }

        [HttpPost]
        public override ActionResult SendRequest(Uri callbackUri)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1} {2}", Request.QueryString["error_reason"], Request.QueryString["error"],
                                          Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var client = new FacebookClient(GetTokenManager());
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
            return Redirect(client.StartAuthentication(callbackUrl, "email, publish_stream"));
        }

        [HttpPost]
        public ActionResult PostComment(Uri callbackUri, string commentText, int EntityId, int EntityType, string isPrivate)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["error"]))
                return
                    Content(String.Format("{0} {1} {2}", Request.QueryString["error_reason"], Request.QueryString["error"],
                                          Request.QueryString["error_description"]));
            var manager = new CommentManager();
            var tempId = manager.SaveTemporaryComment(EntityId, EntityType, commentText, isPrivate == isChecked);
            //если ещё не аутентифицировались
            var client = new FacebookClient(GetTokenManager());
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
            return Redirect(client.StartAuthentication(callbackUrl, "email, publish_stream", tempId.ToString()));
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public override ActionResult Callback(Uri callbackUri, int? state)
        {
            int? tempId = state;
            var code = Request.QueryString["code"];
            if (String.IsNullOrEmpty(code)) throw new ArgumentException(Resources.FacebookLogin.ControllerAuthorizationCodeNotReceived);

            //если уже есть код аутентификации, осталось только получить access_token

            var client = new FacebookClient(GetTokenManager());
            AccountInfo accountInfo;
            try
            {
                string callback = Uri.EscapeDataString(Url.ActionAbsolute("Callback")
                    + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
                client.FinishAuthentication(code, callback);
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
                        Login = accountInfo.Email
                    };
                    user.SetForeignAccountId(accountInfo);
                    user.AccountSource = accountInfo.AccountSource;
                    user.Email = accountInfo.Email;
                    user.FacebookUsername = accountInfo.UserName;
                    user.FacebookProfileUrl = accountInfo.ProfileUrl;

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
                user.AccessTokenFacebook = client.AccessToken;
                user.LastLoginSource = AccountSource.Facebook;
                context.SaveChanges();
                userSession.LogIn(user);
                userSession.Sharer = new FacebookSharer(client.AccessToken, user);
            }
            if (tempId != null)
            {
                var manager = new CommentManager();
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
                    Content(String.Format("{0} {1} {2}", Request.QueryString["error_reason"], Request.QueryString["error"],
                                          Request.QueryString["error_description"]));

            //если ещё не аутентифицировались
            var client = new FacebookClient(GetTokenManager());
            var callbackUrl = Uri.EscapeDataString(Url.ActionAbsolute("AccountProved") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));

            return Redirect(client.StartAuthentication(callbackUrl, "email, publish_stream"));
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult AccountProved(Uri callbackUri)
        {
            if (!userSession.IsAuthenticated) 
                throw new InvalidOperationException(Resources.FacebookLogin.ControllerMustRegisterToAddProfiles);
            var code = Request.QueryString["code"];
            //если уже есть код аутентификации, осталось только получить access_token
            if (!String.IsNullOrEmpty(code))
            {
                var client = new FacebookClient(GetTokenManager());
                AccountInfo accountInfo;
                try
                {
                    string callback = Uri.EscapeDataString(Url.ActionAbsolute("AccountProved")
                        + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()));
                    client.FinishAuthentication(code, callback);
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
                    if(context.Users.Any(u => u.FacebookId == accountInfo.Id))
                        return Content(Resources.FacebookLogin.ControllerAccountBelongsToAnotherUser);

                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                   
                    user.SetForeignAccountId(accountInfo);
                    if (user.Email == null) user.Email = accountInfo.Email;
                    user.FacebookUsername = accountInfo.UserName;
                    user.FacebookProfileUrl = accountInfo.ProfileUrl;

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

                    user.AccessTokenFacebook = client.AccessToken;
                    context.SaveChanges();
                }
            }
            return Redirect(callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", new { Id = userSession.CurrentUser.Id}));
        }

        private static void TryAddFriends(FacebookClient client, SideNotesEntities context, User user)
        {
            List<string> friendIds = null;
            try
            {
                friendIds = client.GetFriendIds();
            }
            catch { }
            if (friendIds != null)
            {
                var friends = context.Users.Where(u => friendIds.Contains(u.FacebookId)).ToList();
                friends.ForEach(f => user.Friends.Add(f));
            }
        }
    }
}
