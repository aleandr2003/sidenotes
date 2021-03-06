﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Models;
using SideNotes.Models.Helpers;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.ViewModels;
using SideNotes.Models.Queries;
using System.Configuration;
using System.Linq.Expressions;
using SideNotes.Extensions;
using SideNotes.Controllers.Abstract;
using System.Web.Routing;

namespace SideNotes.Controllers
{
    public class BookController : SidenotesController
    {
        private IUserSession userSession;
        private IAuthorizationService authz;

        private const int recentCommentsCount = 3;
        private const string on = "on";

        protected int AllCommentsListPageSize
        {
            get
            {
                if (HttpContext.Application["AllCommentsListPageSize"] != null)
                {
                    return (int)HttpContext.Application["AllCommentsListPageSize"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["AllComments_PageSize"], out value)) value = 10;
                    HttpContext.Application["AllCommentsListPageSize"] = value;
                    return value;
                }
            }
        }

        protected int BookPageSize
        {
            get
            {
                if (HttpContext.Application["BookPageSize"] != null)
                {
                    return (int)HttpContext.Application["BookPageSize"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["Book_PageSize"], out value)) value = 10;
                    HttpContext.Application["BookPageSize"] = value;
                    return value;
                }
            }
        }

        protected int ShowCommentators
        {
            get
            {
                if (HttpContext.Application["ShowCommentators"] != null)
                {
                    return (int)HttpContext.Application["ShowCommentators"];
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse(ConfigurationManager.AppSettings["ShowCommentators"], out value)) value = 3;
                    HttpContext.Application["ShowCommentators"] = value;
                    return value;
                }
            }
        }

        public BookController(IUserSession userSession, IAuthorizationService authz)
        {
            this.authz = authz;
            this.userSession = userSession;
            ViewData["SelectedTab"] = HeaderTabs.Catalog;
        }

        public ActionResult Index()
        {
            List<Book> books = null;
            List<int> ownedBookIds = new List<int>();
            using (var context = new SideNotesEntities())
            {
                books = context.Books.Include("Avatar.Small")
                    .OrderByDescending(b => b.Popularity).ToList();
                if (userSession.IsAuthenticated)
                {
                    ownedBookIds = new GetOwnedBookIdsQuery(userSession.CurrentUser.Id).Load(context);
                }
            }
            return View(Tuple.Create(books, ownedBookIds));
        }

        public ActionResult Start(int id)
        {
            return Redirect(Url.Action("View", "Book", new { id = id, skip = GetAutoSave(id) }, true));
        }

        public ActionResult View(int id, int? skip, string expanded)
        {
            if (skip == null) skip = 0;
            Book book = null;
            int paragraphsCount = 0;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.Include("Avatar.Small").Include("Avatar.Medium").FirstOrDefault(b => b.Id == id);
                if (book == null) return View("BookNotFound");
                paragraphsCount = context.Paragraphs.Count(p => p.Book_Id == book.Id && p.OrderNumber > skip);
            }
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
            int pageSize = BookPageSize;
            ViewBag.PageSize = pageSize;
            NavigationModel nav = new NavigationModel()
            {
                BookId = id,
                HasPrevious = skip > 0,
                HasNext = !(paragraphsCount < pageSize),
                PreviousStart = (skip ?? 0) - pageSize,
                NextStart = (skip ?? 0) + pageSize,
                CurrentStart = (skip ?? 0)
            };
            ViewBag.Expanded = expanded == on;
            return View(Tuple.Create(book, nav));
        }

        public ActionResult Page(int id, int? skip, int? take)
        {
            if (skip == null) skip = 0;
            int pageSize = BookPageSize;
            take = take ?? pageSize;
            if (take > pageSize) take = pageSize;
            List<Paragraph> paragraphs = null;
            using (var context = new SideNotesEntities())
            {
                Book book = null;
                book = context.Books.FirstOrDefault(b => b.Id == id);
                if (book == null) return View("BookNotFound");
                paragraphs = context.Paragraphs.Where(p => p.Book_Id == book.Id && p.OrderNumber > skip)
                    .Take(take ?? pageSize).ToList();
                var firstParagraph = paragraphs.FirstOrDefault();
                if (userSession.IsAuthenticated && firstParagraph != null)
                {
                    AutoSave(id, firstParagraph.Id);
                }
            }
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
            return View(paragraphs);
        }

        private void AutoSave(int BookId, int ParagraphId)
        {
            if (!userSession.IsAuthenticated) return;
            using (var context = new SideNotesEntities())
            {
                var autosave = context.Autosaves.FirstOrDefault(
                                    s => s.Book_Id == BookId
                                    &&
                                    s.Owner_Id == userSession.CurrentUser.Id);
                if (autosave == null)
                {
                    autosave = new Autosave()
                    {
                        Owner_Id = userSession.CurrentUser.Id,
                        Paragraph_Id = ParagraphId,
                        Book_Id = BookId,
                        DateUpdated = DateTime.Now
                    };
                    context.Autosaves.AddObject(autosave);
                }
                else
                {
                    autosave.Paragraph_Id = ParagraphId;
                    autosave.DateUpdated = DateTime.Now;
                }
                context.SaveChanges();
            }
        }

        private int GetAutoSave(int BookId)
        {
            if (userSession.IsAuthenticated)
            {
                using (var context = new SideNotesEntities())
                {
                    var autosave = context.Autosaves.FirstOrDefault(
                                        s => s.Book_Id == BookId
                                        &&
                                        s.Owner_Id == userSession.CurrentUser.Id);
                    if (autosave != null) return autosave.Paragraph.OrderNumber - 1;
                }
            }
            return 0;
        }

        public ActionResult Annotation(int? id)
        {
            if (id == null) return RedirectToAction("Index", "Book");
            Book book = null;
            using (var context = new SideNotesEntities())
            {
                book = context.Books
                    .Include("Avatar.Large").Include("Avatar.Medium")
                    .FirstOrDefault(b => b.Id == id);
            }
            if (book == null) return View("BookNotFound");
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            ViewBag.CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
            return View(book);
        }

        public ActionResult AnnotationShort(int id)
        {
            Book book = null;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.FirstOrDefault(b => b.Id == id);
            }
            if (book == null) return View("BookNotFound");
            return View(book);
        }

        public ActionResult Contents(int Id)
        {
            List<Chapter> chapters = null;
            using (var context = new SideNotesEntities())
            {
                var chaptersDic = (from chap in context.Chapters
                                   join p in context.Paragraphs
                                     on chap.TopParagraph_Id equals p.Id
                                   where chap.Book_Id == Id
                                   select new { chapter = chap, paragraph = p }).ToList();
                chaptersDic.ForEach(i => i.chapter.TopParagraph = i.paragraph);
                chapters = chaptersDic.Select(i => i.chapter).ToList(); 
            }
            Chapter RootChapter = BookIndexBuilder.BuildIndex(chapters);
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            return View("RootChapter", RootChapter);
        }

        public ActionResult ContentsWithComments(int Id)
        {
            List<Chapter> chapters = null;
            var userId = userSession.IsAuthenticated ? (int?) userSession.CurrentUser.Id : null;
            using (var context = new SideNotesEntities())
            {
                var chaptersWithCommentCount
                                = (from chap in context.Chapters
                                   join p in context.Paragraphs
                                     on chap.Id equals p.ChapterId
                                   join h in context.HeadComments
                                    .Where(c => c.EntityType == (int) EntityType.Paragraph
                                            && c.Author_Id != null
                                            && (!c.IsPrivate || c.Author_Id == userId))
                                     on p.Id equals h.EntityId 
                                   where chap.Book_Id == Id
                                   group h by chap into g
                                   select new { chapter = g.Key, count = g.Count() }).ToList();

                var chaptersDic = (from chap in context.Chapters
                                   join p in context.Paragraphs
                                     on chap.TopParagraph_Id equals p.Id
                                   where chap.Book_Id == Id
                                   select new { chapter = chap, paragraph = p }).ToList();

                chaptersDic.ForEach(i => 
                    {
                        i.chapter.TopParagraph = i.paragraph;
                        var item = chaptersWithCommentCount.FirstOrDefault(c => c.chapter.Id == i.chapter.Id);
                        i.chapter.HeadCommentsCount = item == null ? 0 : item.count;
                    });
                chapters = chaptersDic.Select(i => i.chapter).ToList(); 
            }
            Chapter RootChapter = BookIndexBuilder.BuildIndex(chapters);
            ViewBag.IsAuthenticated = userSession.IsAuthenticated;
            return View("RootChapter", RootChapter);
        }

        public ActionResult Bookmarks(int Id)
        {
            List<Bookmark> bookmarks = new List<Bookmark>();
            Book book = null;
            if (userSession.IsAuthenticated)
            {
                using (var context = new SideNotesEntities())
                {
                    book = context.Books.FirstOrDefault(b => b.Id == Id);
                    if (book == null) return View("BookNotFound");
                    bookmarks = (from bm in context.Bookmarks
                                 join p in context.Paragraphs
                                  on bm.Paragraph_Id equals p.Id
                                 where p.Book_Id == Id && bm.Owner_Id == userSession.CurrentUser.Id
                                 select bm).ToList();
                    Paragraph ph;
                    foreach(var bm in bookmarks) 
                        ph = bm.Paragraph;
                }
            }
            return View(Tuple.Create(book, bookmarks));
        }

        [HttpPost]
        public ActionResult AddBookmark(string name, int paragraphId, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Book.ControllerMustLogin);
            Paragraph paragraph = null;
            using (var context = new SideNotesEntities())
            {
                paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == paragraphId);
                if(paragraph == null) throw new ArgumentException(Resources.Book.ControllerParagraphNotFound);
                if(String.IsNullOrEmpty(name))
                {
                    name = Resources.Book.ControllerBookmark + " " + DateTime.Now.ToString("YYYY-MM-dd HH:mm");
                }
                var bookmark = new Bookmark()
                {
                    Owner_Id = userSession.CurrentUser.Id,
                    Name = name,
                    Paragraph_Id = paragraphId
                };
                context.Bookmarks.AddObject(bookmark);
                context.SaveChanges();
            }
            if (json == true) return Json(new { });
            return Redirect(Url.Action("View", "Book", new { Id = paragraph.Book_Id, skip = paragraph.OrderNumber - 1}, true));
        }

        [HttpPost]
        public ActionResult RemoveBookmark(int bookmarkId, bool? json)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Book.ControllerMustLogin);
            int BookId = 0;
            using (var context = new SideNotesEntities())
            {
                var bookmark = context.Bookmarks.FirstOrDefault(b => b.Id == bookmarkId);
                if (bookmark == null) throw new ArgumentException(Resources.Book.ControllerBookMarkNotFound);
                if (!authz.Authorize(Operation.RemoveBookmark, bookmark)) 
                    throw new UnauthorizedAccessException(Resources.Book.ControllerCantDeleteBookMark);
                BookId = bookmark.Paragraph.Book_Id;
                context.Bookmarks.DeleteObject(bookmark);
                context.SaveChanges();
            }
            if (json == true) return Json(new { });
            return Redirect(Url.Action("Annotation", "Book", new { Id = BookId }, true));
        }

        public ActionResult CommentParagraph(int paragraphId)
        {
            if (!userSession.IsAuthenticated) throw new InvalidOperationException(Resources.Book.ControllerMustLogin);
            Paragraph paragraph = null;
            using (var context = new SideNotesEntities())
            {
                paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == paragraphId);
                if (paragraph == null) throw new ArgumentException(Resources.Book.ControllerParagraphNotFound);
                if (!paragraph.IsCommentable) throw new ArgumentException(Resources.Book.ControllerCantCommentParagraph);
            }
            return View(paragraph);
        }

        public ActionResult Comments(int paragraphId, int? filter)
        {
            Paragraph paragraph = null;
            filter = filter ?? ((int)CommentFilter.All);
            using (var context = new SideNotesEntities())
            {
                paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == paragraphId);
                if (paragraph == null) throw new ArgumentException(Resources.Book.ControllerParagraphNotFound);
            }
            ViewBag.Filter = filter;
            return View(paragraph);
        }

        public ActionResult CommentsThread(int headCommentId)
        {
            using (var context = new SideNotesEntities())
            {
                var headComment = context.HeadComments.FirstOrDefault(h => h.Id == headCommentId);
                if (headComment == null) throw new ArgumentException(Resources.Book.ControllerCommentNotFound);
                if (headComment.EntityType != (int)EntityType.Paragraph) 
                    throw new InvalidOperationException(Resources.Book.ControllerCommentDoesNotBelongToParagraph);
                var paragraph = context.Paragraphs.Include("Book.Avatar.Small")
                    .Include("Book.Avatar.Medium").FirstOrDefault(p => p.Id == headComment.EntityId);
                if (paragraph == null) throw new ArgumentException(Resources.Book.ControllerParagraphNotFound);

                ViewBag.HeadCommentId = headCommentId;
                return View(paragraph);
            }
        }

        public ActionResult Commentators(int Id)
        {
            using(var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                List<User> headAuthors = (from p in context.Paragraphs
                                           join h in context.HeadComments
                                           .Where(c => c.EntityType == (int)EntityType.Paragraph 
                                                    && c.Author_Id != null
                                                    && (!c.IsPrivate || c.Author_Id == currenUserId))
                                           on p.Id equals h.EntityId
                                           join u in context.Users
                                           on h.Author_Id equals u.Id
                                           where p.Book_Id == Id
                                           select u).Distinct().ToList();

                //List<User> commentAuthors = (from p in context.Paragraphs
                //                             join h in context.HeadComments
                //                             .Where(c => c.EntityType == (int)EntityType.Paragraph 
                //                                    && c.Author_Id != null
                //                                    && (!c.IsPrivate || c.Author_Id == currenUserId))
                //                             on p.Id equals h.EntityId
                //                             join c in context.Comments.Where(c => c.Author_Id != null)
                //                             on h.Id equals c.HeadCommentId
                //                             join u in context.Users
                //                             on c.Author_Id equals u.Id
                //                             where p.Book_Id == Id
                //                             select u).Distinct().ToList();

                List<User> commetators = headAuthors;//.Concat(commentAuthors).Distinct().ToList();
                commetators.Select(u => u.Avatar == null ? null : u.Avatar.Small).ToList();//чтобы загрузить аватары

                List<User> famous = commetators.Where(u => u.IsFamous).ToList();
                List<User> friends = new List<User>();
                if (userSession.IsAuthenticated)
                {
                    List<int> friendIds = context.Users.Include("Friends")
                        .Where(u => u.Id == userSession.CurrentUser.Id)
                        .Select(u => u.Friends).First().Select(f => f.Id).ToList();
                    friends.AddRange(commetators.Where(u => friendIds.Contains(u.Id)));
                }
                List<User> rest = commetators.Where(u => !u.IsFamous && !friends.Contains(u)).ToList();
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                ViewBag.BookId = Id;
                ViewBag.Book = context.Books.Include("Avatar.Medium").FirstOrDefault(b => b.Id == Id);

                return View("CommentatorsPage", Tuple.Create(famous, friends, rest));
            }
        }

        public ActionResult SelectedCommentators(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? currenUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                List<BookCommentatorModel> commetators = (from p in context.Paragraphs
                                                      join h in context.HeadComments
                                                      .Where(c => c.EntityType == (int)EntityType.Paragraph
                                                               && c.Author_Id != null
                                                               && (!c.IsPrivate || c.Author_Id == currenUserId))
                                                      on p.Id equals h.EntityId
                                                      join u in context.Users
                                                      on h.Author_Id equals u.Id
                                                      where p.Book_Id == Id
                                                      group h by u into g
                                                      select new BookCommentatorModel() { user = g.Key, commentCount = g.Count() }).ToList();

                commetators.Select(u => u.user.Avatar == null ? null : u.user.Avatar.Small).ToList();//чтобы загрузить аватары

                List<BookCommentatorModel> famous = commetators.Where(u => u.user.IsFamous).ToList();
                List<BookCommentatorModel> friends = new List<BookCommentatorModel>();
                if (userSession.IsAuthenticated)
                {
                    List<int> friendIds = context.Users.Include("Friends")
                        .Where(u => u.Id == userSession.CurrentUser.Id)
                        .Select(u => u.Friends).First().Select(f => f.Id).ToList();
                    friends.AddRange(commetators.Where(u => friendIds.Contains(u.user.Id)));
                    friends.ForEach(i => i.IsFriend = true);
                }
                List<BookCommentatorModel> rest = commetators.Where(u => !u.user.IsFamous && !friends.Contains(u)).ToList();
                int show = ShowCommentators;
                List<BookCommentatorModel> resultList = friends.Shuffle(new Random()).Take(show).ToList();
                if (resultList.Count < show) resultList = resultList.Concat(famous.Shuffle(new Random()).Take(show)).ToList();
                if (resultList.Count < show) resultList = resultList.Concat(rest.Shuffle(new Random()).Take(show)).ToList();
                resultList = resultList.Take(show).ToList();

                ViewBag.CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                ViewBag.BookId = Id;

                return View(resultList);
            }
        }

        public ActionResult CommentsByUser(int Id, int UserId)
        {
            using (var context = new SideNotesEntities())
            {
                Book book = context.Books.Include("Avatar.Small").Include("Avatar.Medium").FirstOrDefault(b => b.Id == Id);
                if (book == null) return View("BookNotFound");
                User user = context.Users.FirstOrDefault(u => u.Id == UserId);
                if (user == null) throw new ArgumentException(Resources.Book.ControllerUserNotFound);

                return View(Tuple.Create(book, user));
            }
        }

        public ActionResult CommentsByUserPartial(int Id, int UserId)
        {
            using (var context = new SideNotesEntities())
            {
                if (!context.Books.Any(b => b.Id == Id)) return View("BookNotFound");
                User user = context.Users.Include("Avatar.Small").Include("Avatar.Large").FirstOrDefault(u => u.Id == UserId);
                if (user == null) throw new ArgumentException(Resources.Book.ControllerUserNotFound);
                int? currentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                var headComments = (
                                          from p in context.Paragraphs
                                          join h in context.HeadComments.Where(c => c.EntityType == (int)EntityType.Paragraph)
                                          on p.Id equals h.EntityId
                                          where p.Book_Id == Id && h.Author_Id == UserId
                                          && (!h.IsPrivate || h.Author_Id == currentUserId)
                                          select new { comment = h, paragraph = p })
                                          .Distinct().ToDictionary(o => (IComment)o.comment, o => o.paragraph);

                var comments = (from p in context.Paragraphs
                                             join h in context.HeadComments.Where(c => c.EntityType == (int)EntityType.Paragraph)
                                             on p.Id equals h.EntityId
                                             join c in context.Comments
                                             on h.Id equals c.HeadCommentId
                                                where p.Book_Id == Id && c.Author_Id == UserId
                                          select new { comment = c, paragraph = p })
                                          .Distinct().ToDictionary(o => (IComment)o.comment, o => o.paragraph);
                var allCommentList = headComments.Concat(comments).ToDictionary(e => e.Key, e => e.Value);
                    
                return View(Tuple.Create(user, allCommentList));
            }
        }

        public ActionResult AllComments(int Id, int? page)
        {
            using (var context = new SideNotesEntities())
            {
                Book book = context.Books.Include("Avatar.Large").FirstOrDefault(b => b.Id == Id);
                if (book == null) return View("BookNotFound");
                ViewBag.Page = page ?? 1;
                return View(book);
            }
        }

        public ActionResult AllCommentsPartial(int Id, int? page, int? filter)
        {
            CommentFilter Filter = CommentFilter.All;
            try
            {
                Filter = (CommentFilter)(filter ?? ((int)CommentFilter.All));
                if (Filter == CommentFilter.Friends && !userSession.IsAuthenticated) Filter = CommentFilter.All;
            }
            catch
            {
                throw new NotSupportedException(Resources.Book.ControllerFilterTypeNotSupported);
            }
            using (var context = new SideNotesEntities())
            {
                int? currentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                int pageSize = AllCommentsListPageSize;
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
                var headCommentsQuery = (
                                        from p in context.Paragraphs
                                        join h in context.HeadComments
                                            .Where(c => c.EntityType == (int)EntityType.Paragraph)
                                            .Where(filterPredecate)
                                        on p.Id equals h.EntityId
                                        where p.Book_Id == Id && h.Author_Id != null
                                        && (!h.IsPrivate || h.Author_Id == currentUserId)
                                        select new { comment = h, paragraph = p });
                var commentCount = headCommentsQuery.Count();
                var totalPages = commentCount / pageSize + (commentCount % pageSize > 0 ? 1 : 0);
                if (totalPages < page) page = totalPages;
                var skip = ((page ?? 1) - 1) * pageSize;
                skip = skip > 0 ? skip : 0;
                var headComments = headCommentsQuery.OrderByDescending(h => h.comment.DateCreated)
                                        .Skip(skip)
                                        .Take(pageSize)
                                        .ToDictionary(o => (IComment)o.comment, o => o.paragraph);
                foreach (var comment in headComments.Keys)
                {
                    var small = comment.Author.Avatar != null ? comment.Author.Avatar.Small : null;
                }
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.BaseUrl = Request.Url;
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                ViewBag.CurrentUserId = userSession.IsAuthenticated ? userSession.CurrentUser.Id : 0;
                ViewBag.Filter = Filter;
                return View(headComments);
            }
        }

        public ActionResult RecentComments(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                int? userId = null;
                if (userSession.IsAuthenticated) userId = userSession.CurrentUser.Id;
                var headCommentsDic = (from p in context.Paragraphs
                                    join h in context.HeadComments
                                     on p.Id equals h.EntityId
                                    where p.Book_Id == Id
                                    && h.EntityType == (int)EntityType.Paragraph
                                    && h.Author_Id != null
                                    && (!h.IsPrivate || userId == h.Author_Id)
                                    orderby h.DateCreated descending
                                    select new { comment = h, paragraph = p })
                                    .Take(recentCommentsCount).ToDictionary(o => o.comment, o => o.paragraph);
                headCommentsDic.Keys.ToList()
                    .ForEach(h => { var p = h.Author.Avatar != null ? h.Author.Avatar.Small : null; });// чтобы загрузить фотки
                return View(headCommentsDic);                    
            }
        }

        public ActionResult BestComment(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == Id);
                if (book == null) return Content("");
                var bestComment = book.GetBestComment();
                if (bestComment == null) return Content("");
                bestComment = context.HeadComments.FirstOrDefault(h => h.Id == bestComment.Id);
                var temp = (bestComment.Author.Avatar != null ? bestComment.Author.Avatar.Tiny : null);//чтобы подгрузить фотку
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == bestComment.EntityId);
                ViewBag.CurrentUserId = userSession.UserId;

                return View(Tuple.Create(bestComment, paragraph, book));
            }
        }

        public ActionResult NextCommentedParagraph(int Id, int? ParagraphId)
        {
            using (var context = new SideNotesEntities())
            {
                var currentParagraph = context.Paragraphs.FirstOrDefault(p => p.Id == ParagraphId);
                int currentNumber = 0;
                if(currentParagraph != null) currentNumber = currentParagraph.OrderNumber;
                var nextParagraph = (from p in context.Paragraphs
                                     join h in context.HeadComments
                                         on p.Id equals h.EntityId
                                     where h.EntityType == (int)EntityType.Paragraph &&
                                     p.Book_Id == Id &&
                                     p.OrderNumber > currentNumber
                                     orderby p.OrderNumber
                                     select p).FirstOrDefault();
                if (nextParagraph != null)
                {
                    return Redirect(Url.Action("View", "Book", new { Id = Id, skip = nextParagraph.OrderNumber - 1, expanded="on" }, true));
                }
                return Redirect(Url.Action("View", "Book", new { Id = Id, skip = 0, expanded = "on" },true));
            }
        }

        public ActionResult PreviousCommentedParagraph(int Id, int? ParagraphId)
        {
            using (var context = new SideNotesEntities())
            {
                var currentParagraph = context.Paragraphs.FirstOrDefault(p => p.Id == ParagraphId);
                int currentNumber = context.Paragraphs.Where(p => p.Book_Id == Id).Count();
                if (currentParagraph != null) currentNumber = currentParagraph.OrderNumber;
                var nextParagraph = (from p in context.Paragraphs
                                     join h in context.HeadComments
                                         on p.Id equals h.EntityId
                                     where h.EntityType == (int)EntityType.Paragraph &&
                                     p.Book_Id == Id &&
                                     p.OrderNumber < currentNumber
                                     orderby p.OrderNumber descending
                                     select p).FirstOrDefault();
                if (nextParagraph != null)
                {
                    return Redirect(Url.Action("View", "Book", new { Id = Id, skip = nextParagraph.OrderNumber - 1, expanded = "on" },true));
                }
                return Redirect(Url.Action("View", "Book", new { Id = Id, skip = currentNumber - 1, expanded = "on" },true));
            }
        }

        [HttpPost]
        public JsonResult BestCommentJson(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == Id);
                if (book == null) return Json(new { });
                var bestComment = book.GetBestComment();
                if (bestComment == null) return Json(new { });
                bestComment = context.HeadComments.FirstOrDefault(h => h.Id == bestComment.Id);
                var temp = (bestComment.Author.Avatar != null ? bestComment.Author.Avatar.Tiny : null);//чтобы подгрузить фотку
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == bestComment.EntityId);
                ViewBag.CurrentUserId = userSession.UserId;

                return Json(new BestCommentModel(){
                              BookId = book.Id,
                              BookTitle = book.Title,
                              BookAuthor = book.Author,
                              BookAvatarUrl = book.Avatar != null ? VirtualPathUtility.ToAbsolute(book.Avatar.Small.Url) : "",
                              BookReadUrl = Url.Action("Start", "Book", new { Id= book.Id }, true),
                              ParagraphContent = paragraph.Content,
                              ParagraphFormatType = ((int)paragraph.FormatType).ToString(),
                              ParagraphUrl = Url.Action("View", "Book" , new { Id = book.Id, skip= paragraph.OrderNumber - 1 }, true),
                              CommentText = bestComment.Text,
                              UserName = bestComment.Author.Name,
                              UserAvatarUrl = bestComment.Author.Avatar != null ? VirtualPathUtility.ToAbsolute(bestComment.Author.Avatar.Tiny.Url): UserAvatarService.NoAvatarTiny,
                              UserProfileUrl = Url.Action("View", "User" , new {Id = bestComment.Author.Id}),
                              UserCommentsCount = book.GetCommentsCountByAuthor(bestComment.Author_Id ?? 0, userSession.UserId),
                              CommentsByUserUrl = Url.Action("CommentsByUser", "Book", new { Id = book.Id, UserId = bestComment.Author_Id }, true)
                }

                );
            }
        }
        

        public ActionResult Paragraph(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == Id);
                if (paragraph == null) throw new ArgumentException(Resources.Book.ControllerParagraphNotFound);
                ViewBag.IsAuthenticated = userSession.IsAuthenticated;
                ViewBag.CurrentUserId = userSession.IsAuthenticated ? (int?)userSession.CurrentUser.Id : null;
                return View("ParagraphContainer", paragraph);
            }
        }

        public JsonResult CommentCount(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == Id);
                if (book == null) return Json(new {count = 0});

                var count = book.GetCommentsCount(null);
                return Json(new { count = count });
            }
        }

        public ActionResult AcceptDonation(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.Include("Avatar.Large").FirstOrDefault(b => b.Id == Id);
                if (book.PropertyStatus == (int)PropertyStatus.Donatable)
                {
                    return View(book);
                }
                else
                {
                    throw new InvalidOperationException(Resources.Book.ControllerAuthorDoesNotAcceptDonations);
                }
            }
        }
    }

    public class BookCommentatorModel
    {
        public User user;
        public bool IsFriend = false;
        public int commentCount;
    }

    public class BestCommentModel
    {
        public int BookId;
        public string BookTitle;
        public string BookAuthor;
        public string BookAvatarUrl;
        public string BookReadUrl;
        public string ParagraphContent;
        public string ParagraphFormatType;
        public string ParagraphUrl;
        public string CommentText;
        public string UserName;
        public string UserAvatarUrl;
        public string UserProfileUrl;
        public string CommentsByUserUrl;
        public int UserCommentsCount;
    }
}
