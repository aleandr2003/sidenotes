using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.Messaging;
using SideNotes.Extensions;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using SideNotes.OAuth;
using SideNotes.Models;
using SideNotes.Models.Queries;
using System.IO;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class LivejournalLoginController : SidenotesController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;

        public LivejournalLoginController(IUserSession userSession, UserAvatarService avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        public ActionResult GetNickname(Uri callbackUri)
        {
            ViewBag.CallbackUri = callbackUri;
            return View();
        }

        public ActionResult LogOn(Uri callbackUri)
        {
            ViewBag.CallbackUri = callbackUri;
            return View();
        }

        [HttpPost]
        public ActionResult SendRequest(string nickname, Uri callbackUri)
        {
            return StartAuthentication(nickname, callbackUri, AuthenticationOperationType.Login);
        }
        
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult Callback(Uri callbackUri)
        {
            var openid = new OpenIdRelyingParty();
            IAuthenticationResponse response = openid.GetResponse();
            if (response == null) throw new InvalidOperationException("No response received");
            if (response.Status != AuthenticationStatus.Authenticated)
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Canceled:
                        throw new InvalidOperationException("Login was cancelled at the provider");
                    case AuthenticationStatus.Failed:
                        throw new InvalidOperationException("Login failed using the provided OpenID identifier");
                }
            }

            var client = new LivejournalClient(response.ClaimedIdentifier);
            var accountInfo = client.GetAccountInfo();
            using (var context = new SideNotesEntities())
            {
                User user = new GetForeignAccountQuery(accountInfo).Load(context);
                if (user == null)
                {
                    user = new User
                    {
                        Name = accountInfo.Name,
                        Login = "lj_" + accountInfo.UserName
                    };
                    user.SetForeignAccountId(accountInfo);
                    user.AccountSource = accountInfo.AccountSource;
                    user.LivejournalProfileUrl = accountInfo.ProfileUrl;
                    user.LivejournalUsername = accountInfo.UserName;

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
                user.LastLoginSource = AccountSource.Livejournal;
                context.SaveChanges();
                userSession.LogIn(user);
            }

            var returnUrl = callbackUri != null ? callbackUri.ToString() : Url.Action("Index", "Home");
            //return Redirect(returnUrl);
            return RedirectToAction("CompleteExternalRegistration", "User", new { callbackUri = returnUrl });
        }


        [HttpPost]
        public ActionResult ProveAccount(string nickname, Uri callbackUri)
        {
            return StartAuthentication(nickname, callbackUri, AuthenticationOperationType.AccountProof);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult AccountProved(Uri callbackUri)
        {
            if (!userSession.IsAuthenticated)
                throw new InvalidOperationException("Только зарегистрированный пользователь может добавить профиль в соц. сети");
            var openid = new OpenIdRelyingParty();
            IAuthenticationResponse response = openid.GetResponse();

            if (response != null)
            {
                if (response.Status != AuthenticationStatus.Authenticated)
                {
                    switch (response.Status)
                    {
                        case AuthenticationStatus.Canceled:
                            throw new InvalidOperationException("Login was cancelled at the provider");
                        case AuthenticationStatus.Failed:
                            throw new InvalidOperationException("Login failed using the provided OpenID identifier");
                    }
                }
                var client = new LivejournalClient(response.ClaimedIdentifier);
                var accountInfo = client.GetAccountInfo();
                using (var context = new SideNotesEntities())
                {
                    if (context.Users.Any(u => u.LivejournalId == accountInfo.Id))
                        return Content("Этот аккаунт livejournal принадлежит другому пользователю");

                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);

                    user.SetForeignAccountId(accountInfo);
                    user.LivejournalProfileUrl = accountInfo.ProfileUrl;
                    user.LivejournalUsername = accountInfo.UserName;

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
                    context.SaveChanges();
                }

            }

            return Redirect(callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", new { Id = userSession.CurrentUser.Id }));
        }

        private static void TryAddFriends(LivejournalClient client, SideNotesEntities context, User user)
        {
            List<string> friendIds = null;
            try
            {
                friendIds = client.GetFriendIds();
            }
            catch { }
            if (friendIds != null)
            {
                var friends = context.Users.Where(u => friendIds.Contains(u.LivejournalId)).ToList();
                friends.ForEach(f => user.Friends.Add(f));
            }
        }

        private ActionResult StartAuthentication(string nickname, Uri callbackUri, AuthenticationOperationType operation)
        {
            string loginIdentifier = String.Format("http://{0}.livejournal.com", nickname);
            if (!Identifier.IsValid(loginIdentifier))
            {
                ModelState.AddModelError("loginIdentifier",
                            "The specified login identifier is invalid");
                return View();
            }
            else
            {
                string callBackUrl = Url.ActionAbsolute("Callback").ToString();
                if (operation == AuthenticationOperationType.AccountProof)
                    callBackUrl = Url.ActionAbsolute("AccountProved").ToString();
                callBackUrl += "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString());

                var openid = new OpenIdRelyingParty();
                IAuthenticationRequest request = openid.CreateRequest(
                    Identifier.Parse(loginIdentifier),
                    new Realm("http://sidenotes.ru"),
                    new Uri(callBackUrl)
                    );

                return request.RedirectingResponse.AsActionResult();
            }
        }
    }

    public enum AuthenticationOperationType{
        AccountProof,
        Login
    }
}
