using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using SideNotes.OAuth;
using SideNotes.Extensions;
using SideNotes.Models;
using System.IO;
using SideNotes.Models.Queries;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class TwitterLoginController : SidenotesController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;

        public TwitterLoginController(IUserSession userSession, UserAvatarService avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        protected InMemoryTokenManager GetTokenManager()
        {
            return new TwitterTokenManagerFactory().GetTokenManager();
        }

        [HttpPost]
        public ActionResult SendRequest(Uri callbackUri)
        {
            var client = new TwitterClient(GetTokenManager());
            string callBackUrl = Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString());
            client.StartAuthentication(callBackUrl);
            return null;
        }

        [HttpPost]
        public ActionResult PostComment(Uri callbackUri, string commentText, int EntityId, int EntityType, string isPrivate)
        {
            var manager = new CommentManager(null);
            var tempId = manager.SaveTemporaryComment(EntityId, EntityType, commentText, isPrivate == isChecked);
            var client = new TwitterClient(GetTokenManager());
            string callBackUrl = Url.ActionAbsolute("Callback") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString()) + "&tempId=" + tempId;
            client.StartAuthentication(callBackUrl);
            return null;
        }
        public ActionResult Callback(Uri callbackUri, int? tempId)
        {
            var client = new TwitterClient(GetTokenManager());
            var authFinished = client.FinishAuthentication();
            if (!authFinished) return Content("Authentication failed");
           
            AccountInfo accountInfo;
            try
            {
                accountInfo = client.GetAccountInfo(client.UserName);
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
                        Login = "tw_" + accountInfo.UserName
                    };
                    user.SetForeignAccountId(accountInfo);
                    user.AccountSource = accountInfo.AccountSource;
                    user.TwitterUsername = accountInfo.UserName;
                    user.TwitterProfileUrl = accountInfo.ProfileUrl;

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
                user.AccessTokenTwitter = client.AccessToken;
                user.LastLoginSource = AccountSource.Twitter;
                context.SaveChanges();
                userSession.LogIn(user);
                userSession.Sharer = new TwitterSharer(client.AccessToken, user);
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

        public ActionResult ProveAccount(Uri callbackUri)
        {
            var client = new TwitterClient(GetTokenManager());
            string callBackUrl = Url.ActionAbsolute("AccountProved") + "?callbackUri=" + Uri.EscapeDataString(callbackUri.ToString());
            client.StartAuthentication(callBackUrl);
            return null;
        }

        public ActionResult AccountProved(Uri callbackUri)
        {
            if (!userSession.IsAuthenticated)
                throw new InvalidOperationException(Resources.TwitterLogin.ControllerMustRegisterToAddProfiles);
            var client = new TwitterClient(GetTokenManager());
            if (client.FinishAuthentication())
            {
                AccountInfo accountInfo;
                try
                {
                    accountInfo = client.GetAccountInfo(client.UserName);
                }
                catch (InvalidDataException ex)
                {
                    return Content(ex.Message);
                }

                using (var context = new SideNotesEntities())
                {
                    if (context.Users.Any(u => u.TwitterId == accountInfo.Id))
                        return Content(Resources.TwitterLogin.ControllerAccountBelongsToAnotherUser);

                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                    user.SetForeignAccountId(accountInfo);
                    user.TwitterUsername = accountInfo.UserName;
                    user.TwitterProfileUrl = accountInfo.ProfileUrl;

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
                    user.AccessTokenTwitter = client.AccessToken;
                    context.SaveChanges();
                }
            }
            else
            {
                return Content("Authentication failed");
            }
            return Redirect(callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", new { Id = userSession.CurrentUser.Id }));
        }



        private static void TryAddFriends(TwitterClient client, SideNotesEntities context, User user)
        {
            List<string> friendIds = null;
            try
            {
                friendIds = client.GetFriendIds(client.UserName);
            }
            catch { }
            if (friendIds != null)
            {
                var friends = context.Users.Where(u => friendIds.Contains(u.TwitterId)).ToList();
                friends.ForEach(f => user.Friends.Add(f));
            }
        }
    }
}
