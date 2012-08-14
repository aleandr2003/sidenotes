using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.ViewModels;
using SideNotes.Models;
using SideNotes.Services;

namespace SideNotes.Controllers
{
    public class UserAdminController : Controller
    {
        private IAuthorizationService authz;
        private AvatarService userAvatarService;
        public UserAdminController(IAuthorizationService authz, UserAvatarService userAvatarService)
        {
            this.authz = authz;
            this.userAvatarService = userAvatarService;
            ViewData["SelectedTab"] = HeaderTabs.Admin;
        }

        [HttpGet]
        public ActionResult MergeUsers()
        {
            using (var context = new SideNotesEntities())
            {
                var users = context.Users.ToList();
                return View(users);
            }
        }

        [HttpPost]
        public ActionResult MergeUsers(int Id1, int Id2)
        {
            if (!authz.Authorize(Operation.ManageUsers)) throw new UnauthorizedAccessException("Нет прав на управление аккаунтами.");
            using (var context = new SideNotesEntities())
            {
                if (Id1 == Id2) return RedirectToAction("MergeUsers", "UserAdmin");
                var user1 = context.Users.Include("Avatar").FirstOrDefault(u => u.Id == Id1);
                var user2 = context.Users.Include("Avatar").FirstOrDefault(u => u.Id == Id2);
                if (user1 == null) throw new ArgumentException(String.Format("Пользователь {0} не найден", Id1));
                if (user2 == null) throw new ArgumentException(String.Format("Пользователь {0} не найден", Id2));

                if (user1.Avatar_Id == null)
                {
                    user1.Avatar_Id = user2.Avatar_Id;
                }else if (user2.Avatar_Id != null){
                    userAvatarService.RemoveAvatar(user2.Avatar);
                }

                user1.FacebookId = user1.FacebookId ?? user2.FacebookId;
                user1.FacebookProfileUrl = user1.FacebookProfileUrl ?? user2.FacebookProfileUrl;
                user1.FacebookUsername = user1.FacebookUsername ?? user2.FacebookUsername;
                user1.AccessTokenFacebook = user1.AccessTokenFacebook ?? user2.AccessTokenFacebook;
                user1.VkontakteId = user1.VkontakteId ?? user2.VkontakteId;
                user1.VkontakteProfileUrl = user1.VkontakteProfileUrl ?? user2.VkontakteProfileUrl;
                user1.VkontakteUsername = user1.VkontakteUsername ?? user2.VkontakteUsername;
                user1.AccessTokenVkontakte = user1.AccessTokenVkontakte ?? user2.AccessTokenVkontakte;
                user1.TwitterId = user1.TwitterId ?? user2.TwitterId;
                user1.TwitterProfileUrl = user1.TwitterProfileUrl ?? user2.TwitterProfileUrl;
                user1.TwitterUsername = user1.TwitterUsername ?? user2.TwitterUsername;
                user1.AccessTokenTwitter = user1.AccessTokenTwitter ?? user2.AccessTokenTwitter;
                user1.LivejournalId = user1.LivejournalId ?? user2.LivejournalId;
                user1.LivejournalProfileUrl = user1.LivejournalProfileUrl ?? user2.LivejournalProfileUrl;
                user1.LivejournalUsername = user1.LivejournalUsername ?? user2.LivejournalUsername;
                user1.YandexId = user1.YandexId ?? user2.YandexId;
                user1.YandexProfileUrl = user1.YandexProfileUrl ?? user2.YandexProfileUrl;
                user1.YandexUsername = user1.YandexUsername ?? user2.YandexUsername;
                user1.AccessTokenYandex = user1.AccessTokenYandex ?? user2.AccessTokenYandex;
                user1.Email = user1.Email ?? user2.Email;

                var autosaves = context.Autosaves.Where(a => a.Owner_Id == user2.Id).ToList();
                if (autosaves!= null) autosaves.ForEach(a => a.Owner_Id = user1.Id);

                var bookmarks = context.Bookmarks.Where(b =>b.Owner_Id == user2.Id).ToList();
                if (bookmarks != null) bookmarks.ForEach(b => b.Owner_Id = user1.Id);

                var builtIns = context.BuiltInComments.Where(b => b.Author_Id == user2.Id).ToList();
                if (builtIns != null) builtIns.ForEach(b => b.Author_Id = user1.Id);

                var comments = context.Comments.Where(c => c.Author_Id == user2.Id).ToList();
                if (comments != null) comments.ForEach(c => c.Author_Id = user1.Id);

                var headcomments = context.HeadComments.Where(c => c.Author_Id == user2.Id).ToList();
                if (headcomments != null) headcomments.ForEach(c => c.Author_Id = user1.Id);

                var friends = user2.Friends.ToList();
                if (friends != null)
                {
                    foreach (var friend in friends)
                    {
                        if (friend.Id != user1.Id && friend.Id != user2.Id)
                        {
                            user1.Friends.Add(friend);
                        }
                    }
                    user2.Friends.Clear();
                }
                var subscribers = context.Users.Where(u => u.Friends.Select(f => f.Id).Contains(user2.Id)).ToList();
                if (subscribers != null) subscribers.ForEach(u => { u.Friends.Remove(user2); u.Friends.Add(user1); });
                
                context.SaveChanges();
                context.Users.DeleteObject(user2);
                context.SaveChanges();
            }
            return RedirectToAction("MergeUsers", "UserAdmin");
        }

    }
}
