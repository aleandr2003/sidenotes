using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Models;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.ViewModels;
using System.Data;
using SideNotes.Models.Helpers;
using SideNotes.Extensions;
using System.Configuration;
using SideNotes.Controllers.Abstract;
using SideNotes.Repositories;

namespace SideNotes.Controllers
{
    public class UserController : SidenotesController
    {
        readonly IUserSession userSession;
        readonly AvatarService avatarService;
        readonly IAuthorizationService authz;
        readonly IUserRepository userRepository;

        protected int UserListPageSize
        {
            get
            {
                if (HttpContext.Application["UserListPageSize"] != null)
                {
                    return (int)HttpContext.Application["UserListPageSize"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["UserIndex_ItemsPerPage"], out value)) value = 20;
                    HttpContext.Application["UserListPageSize"] = value;
                    return value;
                }
            }
        }

        protected int ShowFriends
        {
            get
            {
                if (HttpContext.Application["ShowFriends"] != null)
                {
                    return (int)HttpContext.Application["ShowFriends"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["ShowFriends"], out value)) value = 3;
                    HttpContext.Application["ShowFriends"] = value;
                    return value;
                }
            }
        }

        protected int RecentCommentsInProfile
        {
            get
            {
                if (HttpContext.Application["RecentCommentsInProfile"] != null)
                {
                    return (int)HttpContext.Application["RecentCommentsInProfile"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["RecentCommentsInProfile"], out value)) value = 3;
                    HttpContext.Application["RecentCommentsInProfile"] = value;
                    return value;
                }
            }
        }

        public UserController(IUserSession userSession,
            UserAvatarService avatarService,
            IAuthorizationService authz,
            IUserRepository userRepository
        )
        {
            this.userSession = userSession;
            this.avatarService = avatarService;
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Users;
            this.userRepository = userRepository;
        }

        public ActionResult Index(int? page)
        {
            if (page == null) page = 1;
            using (var context = new SideNotesEntities())
            {
                var friendIds = new List<int>();
                if (userSession.IsAuthenticated)
                {
                    var user = context.Users.First(u => u.Id == userSession.UserId);
                    friendIds.AddRange(user.Friends.Select(u => u.Id));
                }
                var list = context.Users.Include("Avatar.Small").OrderByDescending(u => u.Id)
                    .Skip(((page ?? 1) - 1) * UserListPageSize)
                    .Take(UserListPageSize).ToList();
                int userCount = context.Users.Count();
                ViewBag.TotalPages = userCount / UserListPageSize + (userCount % UserListPageSize > 0 ? 1 : 0);
                ViewBag.CurrentPage = page;
                ViewBag.BaseUrl = Url.ActionAbsolute("Index", "User");
                ViewBag.FriendIds = friendIds;
                ViewBag.CurrentUserId = userSession.UserId;
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                return View(list);
            }
        }

        [HttpGet]
        public ActionResult SetAvatar()
        {
            if (!userSession.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", null);
            }
            return View(userSession.CurrentUser);
        }

        [HttpPost]
        public ActionResult SetAvatar(int id, HttpPostedFileBase file, bool? json)
        {
            User user = null;
            using (var context = new SideNotesEntities())
            {
                user = context.Users.First(u => u.Id == id);
            }
            if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
            if (!authz.Authorize(Operation.EditUser, user)) 
                throw new UnauthorizedAccessException(Resources.User.ControllerNoPermissionsToEditUser);
            if (file != null)
            {
                avatarService.UploadNew(id, file.InputStream);
            }
            if (json == true) return Json(new { });
            return RedirectToAction("View", new { Id = id });
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model, string returnUrl, bool? json)
        {
            if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
            using (var context = new SideNotesEntities())
            {
                try
                {
                    if (context.Users.Any(u => u.Login == model.Email))
                        throw new ArgumentException(Resources.User.ControllerEmailIsAlreadyUsed);
                    var user = new User
                    {
                        Name = model.Name,
                        Login = model.Email,
                        Email = model.Email,
                        AccountSource = AccountSource.SideNotes,
                        NotifyAuthorCommentReplied = true
                    };
                    user.SetPassword(model.Password);
                
                    context.Users.AddObject(user);
                    context.SaveChanges();
                    userSession.LogIn(user);
                    if (String.IsNullOrEmpty(returnUrl)) returnUrl = Url.Action("View", "User", new { id = user.Id });
                    if (json == true) return Json(new { RedirectUrl = returnUrl });
                    else return Redirect(returnUrl);
                }
                catch(Exception ex)
                {
                    if (ex is UpdateException ||
                        ex is ArgumentException)
                    {
                        if (json == true) return Json(new { ErrorMessage = Resources.User.ControllerEmailIsAlreadyUsed });
                        else return RedirectToAction("Index", "Error", new { message = Resources.User.ControllerEmailIsAlreadyUsed });
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult MakeFriend(int UserId, bool? json)
        {
            if(!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == UserId);
                var currentUser = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
                currentUser.Friends.Add(user);
                context.SaveChanges();
            }
            var url = Url.Action("View", "User", new { id = userSession.CurrentUser.Id });
            if (json == true) return Json(new { RedirectUrl = url});
            return Redirect(url);
        }

        [HttpPost]
        public ActionResult RemoveFriend(int UserId, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == UserId);
                var currentUser = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
                if (currentUser.Friends.Contains(user))
                {
                    currentUser.Friends.Remove(user);
                    context.SaveChanges();
                }
            }
            var url = Url.Action("View", "User", new { id = userSession.CurrentUser.Id });
            if (json == true) return Json(new { RedirectUrl = url });
            return Redirect(url);
        }

        public ActionResult View(int Id)
        {
            User user = null;
            using (var context = new SideNotesEntities())
            {
                user = context.Users
                    .Include("Avatar.Large")
                    .Include("Avatar.Small")
                    .Include("Avatar.Original")
                    .Include("Friends").FirstOrDefault(u => u.Id == Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
                if (userSession.IsAuthenticated && user.Id != userSession.CurrentUser.Id)
                {
                    ViewBag.IsMyFriend = userSession.CurrentUser.IsMyFriend(Id);
                }
            }
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.EditMode = userSession.IsAuthenticated && user.Id == userSession.CurrentUser.Id;
            ViewBag.IsMyProfile = userSession.IsAuthenticated && user.Id == userSession.CurrentUser.Id;

            return View(user);
        }

        [HttpGet]
        public ActionResult CompleteExternalRegistration(Uri callbackUri)
        {
            User user = null;
            if (!userSession.IsAuthenticated) 
                throw new InvalidOperationException(Resources.User.ControllerUserNotRegistered);
            int Id = userSession.CurrentUser.Id;
            using (var context = new SideNotesEntities())
            {
                user = context.Users
                    .Include("Avatar.Large")
                    .Include("Avatar.Small")
                    .Include("Avatar.Original")
                    .Include("Friends").FirstOrDefault(u => u.Id == Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
            }
            ViewBag.CallbackUri = callbackUri != null ? callbackUri.ToString() : Url.Action("View", "User", 
                new { Id = userSession.CurrentUser.Id });
            if (!String.IsNullOrEmpty(user.Email)) return Redirect(ViewBag.CallbackUri);

            return View(user);
        }
        [HttpPost]
        public ActionResult CompleteExternalRegistration(EditProfileModel model, string callbackUri, bool? json)
        {
            User user = null;
            if (!userSession.IsAuthenticated)
                throw new InvalidOperationException(Resources.User.ControllerUserNotRegistered);
            if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
            using (var context = new SideNotesEntities())
            {
                user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                user.Name = model.Name;
                user.Email = model.Email;
                context.SaveChanges();
            }
            if(callbackUri == null) callbackUri = Url.Action("View", "User", new { Id = userSession.CurrentUser.Id });
            if (json == true) return Json(new { RedirectUrl = callbackUri});
            return Redirect(callbackUri);
        }

        public ActionResult RecentReadBooks(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                ViewBag.IsOwner = false;
                if (userSession.IsAuthenticated && userSession.CurrentUser.Id == Id)
                {
                    ViewBag.IsOwner = true;
                }
                var books = (from b in context.Books
                             join a in context.Autosaves
                                 on b.Id equals a.Book_Id
                             where a.Owner_Id == Id
                             orderby a.DateUpdated descending
                             select b).Take(3).ToList();
                books.ForEach(b => { var p = b.Avatar != null ? b.Avatar.Medium : null; }); //чтобы подгрузить картинки
                return View(books);          
            }
        }

        public ActionResult AllReadBooks(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                ViewBag.IsOwner = false;
                if (userSession.IsAuthenticated && userSession.CurrentUser.Id == Id)
                {
                    ViewBag.IsOwner = true;
                }
                int? userId = userSession.UserId;
                var books = (from b in context.Books
                             join a in context.Autosaves on b.Id equals a.Book_Id
                             where a.Owner_Id == Id
                             orderby a.DateUpdated descending
                             select b).ToList();
                books.ForEach(b => { var p = b.Avatar != null ? b.Avatar.Small : null; }); //чтобы подгрузить картинки
                return View(books);
            }
        }

        [HttpPost]
        public ActionResult RemoveMyBook(int BookId)
        {
            if (userSession.IsAuthenticated)
            {
                using (var context = new SideNotesEntities())
                {
                    var autosave = context.Autosaves
                        .FirstOrDefault(a => a.Owner_Id == userSession.CurrentUser.Id
                        && a.Book_Id == BookId);
                    if (autosave != null)
                    {
                        context.Autosaves.DeleteObject(autosave);
                        context.SaveChanges();
                    }
                }
                return RedirectToAction("View", "User", new { Id = userSession.CurrentUser.Id });
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            if(!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            using (var context = new SideNotesEntities())
            {
                ViewBag.CurrentUser = context.Users.Include("Avatar.Large")
                    .FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                return View("EditProfileView",
                    new EditProfileModel {
                        Name = ViewBag.CurrentUser.Name,
                        Email = ViewBag.CurrentUser.Email,
                    });
            }
        }

        [HttpPost]
        public ActionResult EditProfile(EditProfileModel model, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                user.Name = model.Name;
                user.Email = model.Email;

                context.SaveChanges();

                if (json == true) return Json(new { RedirectUrl = Url.Action("View", "User", new {Id = user.Id}) });
                return RedirectToAction("View", "User", new { Id = user.Id });
            }
        }

        [HttpGet]
        public ActionResult EditSettings()
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            using (var context = new SideNotesEntities())
            {
                ViewBag.CurrentUser = context.Users.Include("Avatar.Large")
                    .FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                return View("EditSettingsView",
                    new EditSettingsModel
                    {
                        UrlName = ViewBag.CurrentUser.UrlName
                    });
            }
        }

        [HttpPost]
        public ActionResult EditSettings(EditSettingsModel model, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                if (!String.IsNullOrEmpty(model.UrlName) && !this.userRepository.IsUrlNameAvailable(user.Id, model.UrlName))
                {
                    throw new ArgumentException(Resources.User.UrlNameAlreadyInUse, "UrlName");
                }

                user.UrlName = model.UrlName;
                context.SaveChanges();

                if (json == true) return Json(new { RedirectUrl = Url.Action("View", "User", new { Id = user.Id }) });
                return RedirectToAction("View", "User", new { Id = user.Id });
            }
        }

        [HttpGet]
        public ActionResult UpdateNotificationSettings()
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            using (var context = new SideNotesEntities())
            {
                ViewBag.CurrentUser = context.Users.Include("Avatar.Large")
                    .FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                return View("UpdateNotificationSettingsView",
                    new NotificationSettingsModel(ViewBag.CurrentUser));
            }
        }

        [HttpPost]
        public ActionResult UpdateNotificationSettings(NotificationSettingsModel model, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.User.ControllerMustLogin);
            if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);

                user.NotifyAuthorCommentReplied = model.NotifyAuthorCommentReplied;

                context.SaveChanges();
            }
            if (json == true) return Json(new { });
            else return RedirectToAction("View", "User", new { Id = userSession.CurrentUser.Id});
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!userSession.IsAuthenticated) throw new ArgumentException(Resources.User.ControllerMustLogin);
            return View("ChangePasswordView");
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model, bool? json)
        {
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException(Resources.User.ControllerInvalidFormat);
                if (model.NewPassword != model.ConfirmNewPassword) 
                    throw new ArgumentException(Resources.User.ControllerPasswordConfirmationDoesntMatch);
                if (!userSession.IsAuthenticated) throw new ArgumentException(Resources.User.ControllerMustLogin);
                
                if (userSession.CurrentUser.AccountSource != AccountSource.SideNotes) 
                    throw new InvalidOperationException(Resources.User.ControllerCantChangePasswordOnExternalAccount);
                using (var context = new SideNotesEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                    if (!userSession.CurrentUser.PasswordMatches(model.OldPassword))
                        throw new ArgumentException(Resources.User.ControllerCurrentPasswordDoesntMatch);
                    user.SetPassword(model.NewPassword, model.OldPassword);
                    context.SaveChanges();
                }
            }
            catch (ArgumentException ex)
            {
                if (json == true) return Json(new { ErrorMessage = ex.Message});
                else
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View("ChangePasswordView");
                }
            }
            catch (InvalidOperationException ex)
            {
                if (json == true) return Json(new { ErrorMessage = ex.Message});
                else
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View("ChangePasswordView");
                }
            }
            if (json == true) return Json(new { RedirectUrl = Url.ActionAbsolute("View", "User",
                new { Id = userSession.CurrentUser.Id }) });
            return RedirectToAction("View", "User", new { Id = userSession.CurrentUser.Id});
        }

        public ActionResult BestComment(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? userId = null;
                if (userSession.IsAuthenticated) userId = userSession.CurrentUser.Id;
                var headCommentsDic = (
                                       from h in context.HeadComments
                                       join c in context.Comments
                                        on h.Id equals c.HeadCommentId
                                       where h.Author_Id == Id
                                       && h.EntityType == (int)EntityType.Paragraph
                                       && h.Author_Id != null
                                       && (!h.IsPrivate || userId == h.Author_Id)
                                       group c by h into g
                                       select new { comment = g.Key, children = g.Count() });

                HeadComment bestComment = null;
                if (headCommentsDic.Any())
                {
                    int maxChildren = headCommentsDic.Select(i => i.children).Max();
                    bestComment = headCommentsDic.Where(i => i.children == maxChildren)
                                            .Select(i => i.comment).FirstOrDefault();
                }
                else
                {
                    bestComment = (from h in context.HeadComments
                                       where h.Author_Id == Id
                                       && h.EntityType == (int)EntityType.Paragraph
                                       && h.Author_Id != null
                                       && (!h.IsPrivate || userId == h.Author_Id)
                                       select h).FirstOrDefault();
                }
                if (bestComment == null) return Content("");
                var temp = (bestComment.Author.Avatar != null ? bestComment.Author.Avatar.Tiny : null);//чтобы подгрузить фотку
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == bestComment.EntityId);
                var book = paragraph.Book;
                ViewBag.CurrentUserId = userId;

                return View(Tuple.Create(bestComment, paragraph, book));
            }
        }

        public ActionResult FriendsListShort(int Id)
        {
            ViewBag.UserId = Id;
            return View();
        }

        public ActionResult FriendsPage(int Id, int? Page)
        {
            using (var context = new SideNotesEntities())
            {
                int skip = ((Page ?? 1) - 1) * ShowFriends;
                var user = context.Users.FirstOrDefault(u => u.Id == Id);
                if (user == null) throw new ArgumentException(Resources.User.ControllerUserNotFound);
                var friends = user.Friends.Skip(skip).Take(ShowFriends).ToList();
                friends.ForEach(f => { var photo = f.Avatar == null ? null : f.Avatar.Small; });//подгружаем аватары

                return View(friends);
            }
        }

        public ActionResult RecentComments(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                ViewBag.UserId = Id;
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                return View();
            }
        }

        public ActionResult CommentsPage(int Id, int? Page)
        {
            int skip = ((Page ?? 1) - 1) * RecentCommentsInProfile;
            using (var context = new SideNotesEntities())
            {
                int? userId = userSession.UserId;
                var commentsDic = (from h in context.HeadComments
                                   join p in context.Paragraphs
                                     on h.EntityId equals p.Id
                                   join b in context.Books
                                     on p.Book_Id equals b.Id
                                   where h.EntityType == (int)EntityType.Paragraph
                                     && (!h.IsPrivate || h.Author_Id == userId)
                                     && h.Author_Id == Id
                                   orderby h.DateCreated descending
                                   select new RecentCommentModel() { comment = h, paragraph = p, book = b })
                                   .Skip(skip)
                                   .Take(RecentCommentsInProfile)
                                   .ToList();
                commentsDic.ForEach(c => {
                    c.commentCount = c.book.GetCommentsCountByAuthor(Id, userId);
                    var photo = c.book.Avatar.Small;// чтобы подгрузить аватары
                });
                ViewBag.UserId = Id;
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;

                return View(commentsDic);
            }
        }
    }

    public class RecentCommentModel
    {
        public HeadComment comment;
        public Paragraph paragraph;
        public Book book;
        public int commentCount;
    }
}
