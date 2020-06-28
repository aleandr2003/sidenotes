using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.Models.Helpers;
using SideNotes.Services;
using System.Transactions;
using SideNotes.Models.Queries;
using System.Linq.Expressions;
using SideNotes.ViewModels;
using SideNotes.Controllers.Abstract;
using SideNotes.Services.Templates;

namespace SideNotes.Controllers
{
    public class CommentController : SidenotesController
    {
        
        private int shownCommentators = 12;
            
        private IAuthorizationService authz;
        private IUserSession userSession;
        private ICommentManager commentManager;

        public CommentController(IAuthorizationService authz, IUserSession userSession, ICommentManager manager)
        {
            this.userSession = userSession;
            this.authz = authz;
            this.commentManager = manager;
            ViewData["SelectedTab"] = HeaderTabs.Catalog;
        }

        public ActionResult Index(int entityType, int entityId, int? filter, bool? json)
        {
            if (json == false)
            {
                ViewBag.NoJS = true;
            }
            CommentFilter Filter = CommentFilter.All;
            try
            {
                Filter = (CommentFilter)(filter ?? ((int)CommentFilter.All));
                if (Filter == CommentFilter.Friends && !userSession.IsAuthenticated) Filter = CommentFilter.All;
            }
            catch
            {
                throw new NotSupportedException(Resources.Comment.ControllerUnknownFilterType);
            }

            List<HeadComment> comments = null;
            using (var context = new SideNotesEntities())
            {
                int? CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                Expression<Func<HeadComment, bool>> filterPredecate = c => true;
                if (Filter == CommentFilter.Friends)
                {
                    var FriendIds = context.Users.First(u => u.Id == userSession.CurrentUser.Id)
                        .Friends.Select(f => f.Id).ToList();
                    filterPredecate = c => c.Author_Id != null && FriendIds.Contains((int)c.Author_Id);
                }
                else if (Filter == CommentFilter.Famous)
                {
                    filterPredecate = c => c.Author_Id != null && c.Author.IsFamous;
                }
                comments = context.HeadComments
                    .Include("Author.Avatar.Small")
                    .Where(
                        c => c.EntityType == entityType
                            &&
                            c.EntityId == entityId
                            &&
                            (!c.IsPrivate || c.Author_Id == CurrentUserId)
                    )
                    .Where(filterPredecate)
                    .OrderByDescending(c => c.DateCreated).ToList();
            }
            ViewBag.EntityType = entityType;
            ViewBag.EntityId = entityId;
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.CurrentUserId = userSession.IsAuthenticated ? userSession.CurrentUser.Id : 0;
            ViewBag.Filter = Filter;
            ViewBag.BaseUrl = Request.Url;
            return View("Index", comments);
        }

        public ActionResult HeadIndex(int headCommentId)
        {
            HeadComment headComment = null;
            using (var context = new SideNotesEntities())
            {
                headComment = context.HeadComments.Include("Author.Avatar.Small")
                    .FirstOrDefault(c => c.Id == headCommentId);
                var commentList = context.Comments.Include("Author.Avatar.Small")
                    .Where(c => c.HeadCommentId == headCommentId)
                    .OrderBy(c => c.DateCreated)
                    .ToList();
                headComment.BuildTree(commentList);
            }
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.CurrentUserId = userSession.IsAuthenticated ? userSession.CurrentUser.Id : 0;
            return View(headComment);
        }

        public ActionResult Reply(int commentId, int commentType)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Comment.ControllerNeedLogin);
            int? parentCommentId = null;
            int headCommentId = 0;
            string parentText = String.Empty;
            using (var context = new SideNotesEntities())
            {
                if (commentType == (int)EntityType.Comment)
                {
                    var comment = context.Comments.FirstOrDefault(c => c.Id == commentId);
                    if (comment == null) throw new ArgumentException(Resources.Comment.ControllerCommentNotFound);
                    parentCommentId = comment.Id;
                    headCommentId = comment.HeadCommentId;
                    parentText = comment.Text;
                }
                else if (commentType == (int)EntityType.HeadComment)
                {
                    var comment = context.HeadComments.FirstOrDefault(c => c.Id == commentId);
                    if (comment == null) throw new ArgumentException(Resources.Comment.ControllerCommentNotFound);
                    parentCommentId = null;
                    headCommentId = comment.Id;
                    parentText = comment.Text;
                }
                else
                {
                    throw new NotSupportedException(Resources.Comment.ControllerUnknownCommentType);
                }
            }
            ViewBag.ParentCommentId = parentCommentId;
            ViewBag.HeadCommentId = headCommentId;
            ViewBag.ParentText = parentText;
            return View();
        }

        [HttpGet]
        public ActionResult AddHead(int entityId, int entityType)
        {
            ViewBag.EntityId = entityId;
            ViewBag.EntityType = entityType;

            return View();
        }

        [HttpPost]
        public ActionResult AddHead(int entityId, int entityType, string commentText, string isPrivate, bool? json)
        {
            try
            {
                if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Comment.ControllerNeedLogin);
                commentManager.AddHeadComment(userSession.CurrentUser.Id, entityId, entityType, commentText, isPrivate == isChecked);
            }
            catch (InvalidOperationException ex)
            {
                if (json == true) return Json(new { ErrorMessage = ex.Message });
                else throw;
            }
            //return RedirectToAction("Index", "Comment", new { entityId = entityId, entityType = entityType });
            if (json == true) return Json(new { RedirectUrl = Url.Action("Index", "Comment", new { entityId = entityId, entityType = entityType })});//IndexAll(entityType, entityId);
            return RedirectToAction("Comments", "Book", new { paragraphId = entityId });
        }

        [HttpPost]
        public ActionResult DeleteHead(int Id, bool? json)
        {
            int entityId = 0;
            using (var context = new SideNotesEntities())
            {
                try
                {
                    var comment = context.HeadComments.FirstOrDefault(c => c.Id == Id);
                    if (comment == null) throw new ArgumentException(Resources.Comment.ControllerCommentNotFound);
                    if (!authz.Authorize(Operation.DeleteHeadComment, comment))
                        throw new UnauthorizedAccessException(Resources.Comment.ControllerNoPermissionToDeleteComment);
                    entityId = comment.EntityId;
                    //int entityType = comment.EntityType;
                    using (var scope = new TransactionScope())
                    {
                        if (context.Comments.Any(c => c.HeadCommentId == Id))
                        {
                            comment.Text = $"({Resources.Comment.CommentRemoved})";
                            comment.Author_Id = null;
                        }
                        else
                        {
                            context.DeleteObject(comment);
                        }
                        context.SaveChanges();
                        scope.Complete();
                    }
                }
                catch (ArgumentException ex)
                {
                    if (json == true) return Json(new { ErrorMessage = ex.Message });
                    else throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (json == true) return Json(new { ErrorMessage = ex.Message });
                    else throw;
                }
                if (json == true) return Json(new { });
                return RedirectToAction("Comments", "Book", new { paragraphId = entityId });
            }
        }

        [HttpGet]
        public ActionResult AddComment(int? parentCommentId, int headCommentId)
        {
            ViewBag.ParentCommentId = parentCommentId;
            ViewBag.HeadCommentId = headCommentId;
            var comment = Comment.GetParent(parentCommentId, headCommentId);
            if (comment == null) throw new InvalidOperationException(Resources.Comment.ControllerCommentNotFound);
            if (comment.IsDeleted) throw new InvalidOperationException(Resources.Comment.ControllerCommentRemoved);
            return View();
        }

        [HttpPost]
        public ActionResult AddComment(int? parentCommentId, int headCommentId, string commentText, bool? json)
        {
            try
            {
                if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Comment.ControllerNeedLogin);
                commentManager.AddComment(userSession.CurrentUser.Id, parentCommentId, headCommentId, commentText);
            }
            catch (InvalidOperationException ex)
            {
                if (json == true) return Json(new { ErrorMessage = ex.Message });
                else throw;
            }
            if (json == true) return Json(new { RedirectUrl = Url.Action("CommentsThread", "Book", new { headCommentId = headCommentId }) });
            //return RedirectToAction("HeadIndex", "Comment", new { headCommentId = headCommentId });
            return RedirectToAction("CommentsThread", "Book", new { headCommentId = headCommentId });
        }

        [HttpPost]
        public ActionResult DeleteComment(int Id, bool? json)
        {
            HeadComment headComment = null;
            using (var context = new SideNotesEntities())
            {
                try
                {
                    var comment = context.Comments.FirstOrDefault(c => c.Id == Id);
                    if (comment == null) throw new ArgumentException(Resources.Comment.ControllerCommentNotFound);
                    if (!authz.Authorize(Operation.DeleteComment, comment))
                        throw new UnauthorizedAccessException(Resources.Comment.ControllerNoPermissionToDeleteComment);
                    headComment = comment.HeadComment;
                    //int entityType = comment.EntityType;
                    using (var scope = new TransactionScope())
                    {
                        if (context.Comments.Any(c => c.ParentCommentId == Id))
                        {
                            comment.Text = $"({Resources.Comment.CommentRemoved})";
                            comment.Author_Id = null;
                        }
                        else
                        {
                            context.DeleteObject(comment);
                        }
                        context.SaveChanges();
                        scope.Complete();
                    }
                }
                catch (ArgumentException ex)
                {
                    if (json == true) return Json(new { ErrorMessage = ex.Message });
                    else throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (json == true) return Json(new { ErrorMessage = ex.Message });
                    else throw;
                }
                if (json == true) return Json(new { });
                return RedirectToAction("CommentsThread", "Book", new { headCommentId = headComment.Id});
            }
        }

        [HttpPost]
        public ActionResult AddBuiltInComment(int entityId, int entityType, int commentType, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Comment.ControllerNeedLogin);
            if (entityType != (int)EntityType.Paragraph) 
                throw new NotSupportedException(Resources.Comment.ControllerCantAddMark);
            if (!Enum.IsDefined(typeof(BuiltInCommentEnum), commentType)) 
                throw new ArgumentException(Resources.Comment.ControllerUnknownMarkType);
            using (var context = new SideNotesEntities())
            {
                bool exists = context.BuiltInComments.Any(b => 
                    b.ContentNumber == commentType
                    && b.Author_Id == userSession.CurrentUser.Id
                    && b.EntityType == entityType
                    && b.EntityId == entityId);
                if (!exists)
                {
                    var builtIn = new BuiltInComment()
                    {
                        EntityId = entityId,
                        EntityType = entityType,
                        Author_Id = userSession.CurrentUser.Id,
                        ContentNumber = commentType
                    };
                    context.BuiltInComments.AddObject(builtIn);
                    context.SaveChanges();
                }
                if (json == true)
                {
                    int count = context.BuiltInComments.Count(b =>
                       b.ContentNumber == commentType
                       && b.EntityType == entityType
                       && b.EntityId == entityId);
                    return Json(new { Count = count });
                }
                return RedirectToAction("Comments", "Book", new { paragraphId = entityId });
            }
        }

        [HttpPost]
        public ActionResult DeleteBuiltInComment(int entityId, int entityType, int commentType, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Comment.ControllerNeedLogin);
            if (entityType != (int)EntityType.Paragraph)
                throw new NotSupportedException(Resources.Comment.ControllerCantAddMark);
            if (!Enum.IsDefined(typeof(BuiltInCommentEnum), commentType))
                throw new ArgumentException(Resources.Comment.ControllerUnknownMarkType);
            using (var context = new SideNotesEntities())
            {
                var builtIn = context.BuiltInComments.FirstOrDefault(b =>
                    b.ContentNumber == commentType
                    && b.Author_Id == userSession.CurrentUser.Id
                    && b.EntityType == entityType
                    && b.EntityId == entityId);
                if (builtIn != null)
                {
                    context.BuiltInComments.DeleteObject(builtIn);
                    context.SaveChanges();
                }
                if (json == true)
                {
                    int count = context.BuiltInComments.Count(b =>
                    b.ContentNumber == commentType
                    && b.EntityType == entityType
                    && b.EntityId == entityId);
                    return Json(new { Count = count });
                }
                return RedirectToAction("Comments", "Book", new { paragraphId = entityId });
            }
        }

        public ActionResult IndexBuiltIn(int entityId, int entityType)
        {
            if (userSession.IsAuthenticated)
            {
                return ListBuiltInEdit(entityId, entityType);
            }
            else
            {
                return ListBuiltIn(entityId, entityType);
            }
        }

        public ActionResult ListBuiltIn(int entityId, int entityType)
        {
            ViewBag.EntityId = entityId;
            ViewBag.EntityType = entityType;
            using (var context = new SideNotesEntities())
            {
                Dictionary<BuiltInCommentEnum, int> builtInList =
                            (from b in context.BuiltInComments
                             where b.EntityId == entityId && b.EntityType == entityType
                             group b by b.ContentNumber into g
                             select new { key = g.Key, number = g.Count() })
                             .ToDictionary(g => (BuiltInCommentEnum)g.key, g => g.number);
                return View("IndexBuiltIn", builtInList);
            }
        }

        public ActionResult ListBuiltInEdit(int entityId, int entityType)
        {
            if (!userSession.IsAuthenticated) throw new UnauthorizedAccessException(Resources.Comment.ControllerNeedLogin);
            ViewBag.EntityId = entityId;
            ViewBag.EntityType = entityType;
            using (var context = new SideNotesEntities())
            {
                Dictionary<BuiltInCommentEnum, int> builtInList =
                            (from b in context.BuiltInComments
                             where b.EntityId == entityId && b.EntityType == entityType
                             group b by b.ContentNumber into g
                             select new { key = g.Key, number = g.Count() })
                             .ToDictionary(g => (BuiltInCommentEnum)g.key, g => g.number);
                
                Dictionary<BuiltInCommentEnum, int> resultList = new Dictionary<BuiltInCommentEnum, int>();
                foreach (BuiltInCommentEnum key in Enum.GetValues(typeof(BuiltInCommentEnum)))
                {
                    int count = 0;
                    if (builtInList.Keys.Contains(key))
                    {
                        count = builtInList[key];
                    }
                    resultList[key] = count;
                }

                List<BuiltInCommentEnum> setList =
                            (from b in context.BuiltInComments
                                where b.EntityId == entityId && b.EntityType == entityType
                                && b.Author_Id == userSession.CurrentUser.Id
                                select (BuiltInCommentEnum)b.ContentNumber).Distinct().ToList();
                return View("IndexBuiltInEdit", Tuple.Create(resultList, setList));
            }
        }

        public JsonResult EntityCommentators(int entityId, int entityType, List<int> UserIds, int? take)
        {
            using (var context = new SideNotesEntities())
            {
                List<int> commentatorIds = new List<int>();
                if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                    commentatorIds.Add(userSession.CurrentUser.Id);
                int? currentUserId = userSession.IsAuthenticated ? (int?) userSession.CurrentUser.Id : null;
                var shownCommentatorIds = new EntityCommentatorsQuery(currentUserId, entityId, entityType, take ?? shownCommentators, commentatorIds).Load(context);
                var users = context.Users.Include("Avatar.Tiny").Where(u => shownCommentatorIds.Contains(u.Id)).ToList();
                var modelList = 
                            users.Select(u => new CommentatorModel()
                            {
                                Id = u.Id,
                                Name = u.Name,
                                AvatarUrl = u.Avatar != null && u.Avatar.Tiny != null ? VirtualPathUtility.ToAbsolute(u.Avatar.Tiny.Url) : UserAvatarService.NoAvatarTiny,
                                IsFamous = u.IsFamous,
                                IsFriend = false,
                                ProfileUrl = Url.Action("View", "User", new { Id = u.Id })
                            }).ToList();
                //if (userSession.IsAuthenticated)
                //{
                //    var user = context.Users.First(u => u.Id == userSession.CurrentUser.Id);
                //    List<int> friendIds = user.Friends.Select(f => f.Id).ToList();
                //    modelList.ForEach(model => model.IsFriend = friendIds.Contains(model.Id));
                //}
                return Json(new { EntityId = entityId, EntityType = entityType, userList = modelList });
            }
        }

        public JsonResult EntityCommentatorsList(List<int> entityIds, int entityType, List<int> UserIds, int? take)
        {
            using (var context = new SideNotesEntities())
            {
                if (entityIds == null) return Json(new List<int>());
                List<int> commentatorIds = new List<int>();
                if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                    commentatorIds.Add(userSession.CurrentUser.Id);
                int? currentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                var commentatorsDic = new EntityListCommentatorsQuery(currentUserId, entityIds, entityType, take ?? shownCommentators, commentatorIds).Load(context);
                foreach (var item in entityIds) { if (!commentatorsDic.ContainsKey(item)) { commentatorsDic[item] = new List<int>(); }}
                var shownCommentatorIds = commentatorsDic.SelectMany(i => i.Value).Distinct().ToList();
                var users = context.Users.Include("Avatar.Tiny").Where(u => shownCommentatorIds.Contains(u.Id)).ToList();
                var modelList =
                            users.Select(u => new CommentatorModel()
                            {
                                Id = u.Id,
                                Name = u.Name,
                                AvatarUrl = u.Avatar != null && u.Avatar.Tiny != null ? VirtualPathUtility.ToAbsolute(u.Avatar.Tiny.Url) : UserAvatarService.NoAvatarTiny,
                                IsFamous = u.IsFamous,
                                IsFriend = false,
                                ProfileUrl = Url.Action("View", "User", new { Id = u.Id })
                            }).ToList();
                var result = commentatorsDic.Select(i =>
                    new
                    {
                        EntityId = i.Key,
                        EntityType = entityType,
                        userList = i.Value.Select(Id => modelList.FirstOrDefault(u => u.Id == Id)).ToList()
                    }).ToList();
                //if (userSession.IsAuthenticated)
                //{
                //    var user = context.Users.First(u => u.Id == userSession.CurrentUser.Id);
                //    List<int> friendIds = user.Friends.Select(f => f.Id).ToList();
                //    modelList.ForEach(model => model.IsFriend = friendIds.Contains(model.Id));
                //}
                return Json(result);
            }
        }

        public ActionResult ThreadCommentators(int headCommentId)
        {
            using (var context = new SideNotesEntities())
            {
                int? currentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                var shownCommentatorIds = new ThreadCommentatorsQuery(currentUserId, headCommentId, shownCommentators).Load(context);
                var users = context.Users.Include("Avatar.Small").Where(u => shownCommentatorIds.Contains(u.Id)).ToList();
                return View("CommentatorsList", users);
            }
        }

    }
    public enum CommentFilter
    {
        All,
        Friends,
        Famous
    }

    

}
