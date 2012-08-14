using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.Services;
using SideNotes.Extensions;
using System.Configuration;
using SideNotes.ViewModels;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class FeedController : SidenotesController
    {
        private IUserSession userSession;
        private IAuthorizationService authz;

        protected int FeedPageSize
        {
            get
            {
                if (HttpContext.Application["FeedPageSize"] != null)
                {
                    return (int)HttpContext.Application["FeedPageSize"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["Feed_PageSize"], out value)) value = 10;
                    HttpContext.Application["FeedPageSize"] = value;
                    return value;
                }
            }
        }

        public FeedController(IUserSession userSession, IAuthorizationService authz)
        {
            this.authz = authz;
            this.userSession = userSession;
            ViewData["SelectedTab"] = HeaderTabs.Catalog;
        }

        public ActionResult Index(int Id, int? paragraphId)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == Id);
                if (book == null) throw new ArgumentException("Книга не найдена");
                
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                ViewBag.CurrentUserId = userSession.UserId;
                ViewBag.BookId = Id;
                ViewBag.ParagraphId = paragraphId;
                return View(book);
            }
        }

        public JsonResult Paragraphs(int Id, List<int> UserIds, int? sortType, int? page)
        {
            ParagraphSortType SortType = sortType == null ? ParagraphSortType.ByOrderNumber : (ParagraphSortType)sortType.Value;
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                List<int> commentatorIds = new List<int>();
                if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                    commentatorIds.Add(userSession.CurrentUser.Id);
                var query = (from p in context.Paragraphs
                            join h in context.HeadComments
                            .Where(c => c.EntityType == (int)EntityType.Paragraph
                                    && c.Author_Id != null
                                    && (!c.IsPrivate || c.Author_Id == currenUserId)
                                    && (commentatorIds.Contains(c.Author_Id ?? 0) ))
                            on p.Id equals h.EntityId
                            where p.Book_Id == Id
                            group h by p into g
                            select new { 
                                p = g.Key, 
                                first = g.OrderBy(c => c.DateCreated).FirstOrDefault(),
                                last = g.OrderByDescending(c => c.DateCreated).FirstOrDefault()
                            });
                if(SortType == ParagraphSortType.ByRecentComments){
                    query = query.OrderByDescending(item => item.last.DateCreated);
                }else if(SortType == ParagraphSortType.ByOrderNumber){
                    query = query.OrderBy(item => item.p.OrderNumber);
                }
                int skip = (page != null ? (page - 1) ?? 0 : 0) * FeedPageSize;
                var paragraphs = query.Skip(skip).Take(FeedPageSize);

                List<ParagraphModel> modelList = new List<ParagraphModel>();
                foreach(var item in paragraphs){
                    var p = item.p;
                    var first = item.first;
                    var last = item.last;
                    modelList.Add(new ParagraphModel()
                    {
                        Id = p.Id,
                        BookId = p.Book_Id,
                        OrderNumber = p.OrderNumber,
                        Content = p.Content,
                        FormatType = (int)p.FormatType,
                        BookTextUrl = Url.ActionAbsolute("View", "Book", new {Id = p.Book_Id, skip = p.OrderNumber - 1}).ToString(),
                        FirstComment = new CommentModel(first, Url),
                        LastComment = new CommentModel(last, Url)
                    });
                }
                return Json(modelList);
            }
        }

        public JsonResult SingleParagraph(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == Id);
                if (paragraph == null) return Json(null);
                var firstComment = context.HeadComments.Where(h => h.EntityType == (int)EntityType.Paragraph && h.EntityId == Id).OrderBy(h => h.DateCreated).FirstOrDefault();
                var lastComment = context.HeadComments.Where(h => h.EntityType == (int)EntityType.Paragraph && h.EntityId == Id).OrderByDescending(h => h.DateCreated).FirstOrDefault();
                ParagraphModel model = new ParagraphModel()
                {
                    Id = paragraph.Id,
                    BookId = paragraph.Book_Id,
                    OrderNumber = paragraph.OrderNumber,
                    Content = paragraph.Content,
                    FormatType = (int)paragraph.FormatType,
                    BookTextUrl = Url.ActionAbsolute("View", "Book", new { Id = paragraph.Book_Id, skip = paragraph.OrderNumber - 1 }).ToString(),
                    FirstComment = new CommentModel(firstComment, Url),
                    LastComment = new CommentModel(lastComment, Url)
                };
                return Json(model);
            }
        }


        public JsonResult CommentCount(int Id, List<int> UserIds)
        {
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                int count = 0;
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == Id);
                if (paragraph != null)
                {
                    List<int> commentatorIds = new List<int>();
                    if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                    if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                        commentatorIds.Add(userSession.CurrentUser.Id);
                    count = context.HeadComments.Where(
                        c => c.EntityId == Id
                        && c.EntityType == (int)EntityType.Paragraph
                        && c.Author_Id != null
                        && (!c.IsPrivate || c.Author_Id == currenUserId)
                        && (commentatorIds.Contains(c.Author_Id ?? 0))).Count();
                }
                return Json(new { Id, count });
            }
        }

        public JsonResult CommentCountList(List<int> EntityIds, List<int> UserIds)
        {
            if (EntityIds == null) return Json(new { });
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;

                List<int> commentatorIds = new List<int>();
                if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                    commentatorIds.Add(userSession.CurrentUser.Id);
                var result = (from c in context.HeadComments
                             where EntityIds.Contains(c.EntityId)
                                && c.EntityType == (int)EntityType.Paragraph
                                && c.Author_Id != null
                                && (!c.IsPrivate || c.Author_Id == currenUserId)
                                && (commentatorIds.Contains(c.Author_Id ?? 0))
                             group c by c.EntityId into gr
                             select new {Id = gr.Key, count = gr.Count()}).ToList();
                foreach (var item in EntityIds) { if (!result.Any(i => i.Id == item)) { result.Add(new { Id = item, count = 0 }); } }
                return Json(result);
            }
        }

        public JsonResult Commentators(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? currentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                List<User> commetators = (from p in context.Paragraphs
                                          join h in context.HeadComments
                                          .Where(c => c.EntityType == (int)EntityType.Paragraph
                                                   && c.Author_Id != null
                                                   && (!c.IsPrivate || c.Author_Id == currentUserId))
                                          on p.Id equals h.EntityId
                                          join u in context.Users
                                          on h.Author_Id equals u.Id
                                          where p.Book_Id == Id && u.Id != (currentUserId ?? 0)
                                          select u).Distinct().ToList();

                commetators.Select(u => u.Avatar == null ? null : u.Avatar.Small).ToList(); //чтобы загрузить аватары
                List<CommentatorModel> modelList = new List<CommentatorModel>();
                commetators.ForEach(c => 
                    modelList.Add(new CommentatorModel() {
                        Id = c.Id,
                        Name = c.Name,
                        AvatarUrl = c.Avatar != null && c.Avatar.Tiny != null ? VirtualPathUtility.ToAbsolute(c.Avatar.Tiny.Url) : UserAvatarService.NoAvatarTiny,
                        IsFamous = c.IsFamous,
                        IsFriend = false,
                        ProfileUrl = Url.Action("View", "User", new { Id = c.Id })
                    })
                );
                if (userSession.IsAuthenticated)
                {
                    var user = context.Users.First(u => u.Id == userSession.CurrentUser.Id);
                    List<int> friendIds = user.Friends.Select(f => f.Id).ToList();
                    foreach (var model in modelList)
                    {
                        if (friendIds.Contains(model.Id)) model.IsFriend = true;
                    }
                }

                return Json(modelList);
            }
        }

        [HttpPost]
        public JsonResult ParagraphCommentators(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.UserId;
                List<User> commetators = (from h in context.HeadComments
                                          join u in context.Users
                                          on h.Author_Id equals u.Id
                                          where h.EntityId == Id
                                                && h.EntityType == (int)EntityType.Paragraph
                                                && h.Author_Id != null
                                                && (!h.IsPrivate || h.Author_Id == currenUserId)
                                          select u).Distinct().ToList();

                commetators.Select(u => u.Avatar == null ? null : u.Avatar.Small).ToList(); //чтобы загрузить аватары
                List<CommentatorModel> modelList = new List<CommentatorModel>();
                commetators.ForEach(c =>
                    modelList.Add(new CommentatorModel()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AvatarUrl = c.Avatar != null && c.Avatar.Tiny != null ? VirtualPathUtility.ToAbsolute(c.Avatar.Tiny.Url) : UserAvatarService.NoAvatarTiny,
                        IsFamous = c.IsFamous,
                        IsFriend = false,
                        ProfileUrl = Url.Action("View", "User", new { Id = c.Id })
                    })
                );
                if (userSession.IsAuthenticated)
                {
                    var user = context.Users.First(u => u.Id == userSession.CurrentUser.Id);
                    List<int> friendIds = user.Friends.Select(f => f.Id).ToList();
                    foreach (var model in modelList)
                    {
                        if (friendIds.Contains(model.Id)) model.IsFriend = true;
                    }
                }

                return Json(modelList);
            }
        }
        public JsonResult CommentsFiltered(int Id, List<int> UserIds)
        {
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                List<int> commentatorIds = new List<int>();
                if (UserIds != null && UserIds.Count > 0) commentatorIds.AddRange(UserIds);
                if (userSession.IsAuthenticated && !commentatorIds.Contains(userSession.CurrentUser.Id))
                    commentatorIds.Add(userSession.CurrentUser.Id);
                List<HeadComment> comments = context.HeadComments.Include("Author.Avatar.Small")
                                              .Where(h => h.EntityType == (int)EntityType.Paragraph
                                                       && h.EntityId == Id
                                                       && h.Author_Id != null
                                                       && (!h.IsPrivate || h.Author_Id == currenUserId)
                                                       && (commentatorIds.Contains(h.Author_Id ?? 0)))
                                              .OrderBy(h => h.DateCreated)
                                              .ToList();

                List<CommentModel> modelList = new List<CommentModel>();
                comments.ForEach(c => modelList.Add(new CommentModel(c, Url)));
                return Json(new { paragraphId = Id, commentList = modelList });
            }
        }

        public JsonResult GetRepliesCount(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                int count = 0;
                var comment = context.HeadComments.FirstOrDefault(
                    h => h.Id == Id 
                    && h.Author_Id != null
                    && (!h.IsPrivate || h.Author_Id == currenUserId));
                if (comment != null)
                {
                    count = comment.GetChildCommentsCount();
                }
                return Json(new { Id, count });
            }
        }

        public ActionResult Header(int BookId)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.Include("Avatar.Small").Include("Avatar.Medium").FirstOrDefault(b => b.Id == BookId);
                if (book == null) throw new ArgumentException("Книга не найдена");
                return View(book);
            }
        }
    }

    public class CommentModel
    {
        public int Id;
        public string Text;
        public int AuthorId;
        public string AuthorName;
        public string AvatarUrl;
        public string ProfileUrl;
        public int ChildCommentsCount;
        public string RepliesLink;
        public string date;

        public CommentModel() { }
        public CommentModel(HeadComment comment, UrlHelper urlHelper)
        {
            Id = comment.Id;
            Text = comment.Text;
            if (comment.Author != null)
            {
                AuthorId = comment.Author.Id;
                AuthorName = comment.Author.Name;
                AvatarUrl = comment.Author.Avatar != null && comment.Author.Avatar.Tiny != null
                    ? VirtualPathUtility.ToAbsolute(comment.Author.Avatar.Tiny.Url)
                    : UserAvatarService.NoAvatarTiny;
                ProfileUrl = urlHelper.Action("View", "User", new { Id = AuthorId });
            }
            ChildCommentsCount = comment.GetChildCommentsCount();
            RepliesLink = urlHelper.Action("CommentsThread", "Book", new { headCommentId = Id });
            date = comment.DateCreated.ToString("dd MMM yyyy HH:mm");
        }
    }

    public class ParagraphModel
    {
        public int Id;
        public int BookId;
        public int OrderNumber;
        public string Content;
        public int FormatType;
        public string BookTextUrl;
        public CommentModel FirstComment;
        public CommentModel LastComment;
    }

    public enum ParagraphSortType
    {
        ByRecentComments,
        ByOrderNumber
    }
}
