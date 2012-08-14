using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.OAuth;
using SideNotes.Models;
using System.IO;
using System.Net;
using SideNotes.Services.Abstract;
using SideNotes.Models.Queries;
using SideNotes.Services;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public abstract class OAuthLoginController : SidenotesController
    {
        protected abstract InMemoryTokenManager GetTokenManager();

        [HttpPost]
        public abstract ActionResult SendRequest(Uri callbackUri);

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public abstract ActionResult Callback(Uri callbackUri, int? tempId);

        protected void CreateOrAuthorizeUser(AccountInfo accountInfo)
        {
            using (var context = new SideNotesEntities())
            {
                User user = new GetForeignAccountQuery(accountInfo).Load(context);
                if (user == null)
                {
                    user = CreateUser(accountInfo);
                }
                else
                {
                    user.SetForeignAccountId(accountInfo);
                    context.SaveChanges();
                }
                userSession.LogIn(user);
            }
            
        }

        private User CreateUser(AccountInfo accountInfo)
        {
            var user = new User
            {
                Name = accountInfo.Name
            };
            user.SetForeignAccountId(accountInfo);
            user.AccountSource = accountInfo.AccountSource;
            if (accountInfo.AccountSource != AccountSource.Vkontakte)
            {
                user.Email = accountInfo.Email;
            }
            using (var context = new SideNotesEntities())
            {
                context.Users.AddObject(user);
                context.SaveChanges();
            }

            Stream imageStream = accountInfo.GetAvatarInputStream();
            if (imageStream != null)
            {
                avatarService.UploadNew(user.Id, imageStream);
                imageStream.Close();
            }
            return user;
        }

        public OAuthLoginController(IUserSession userSession, UserAvatarService avatarService)
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
        }

        readonly IUserSession userSession;
        readonly AvatarService avatarService;
    }
}
