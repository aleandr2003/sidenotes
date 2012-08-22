using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Models;
using SideNotes.Services;
using System.IO;
using SideNotes.Services.Abstract;
using SideNotes.ViewModels;
using System.Web.Hosting;
using SideNotes.Controllers.Abstract;
using System.Configuration;
using Ionic.Zip;
using System.Text;

namespace SideNotes.Controllers
{
    public class BookAdminController : SidenotesController
    {
        private ParserFactory parserFactory;
        private IAuthorizationService authz;
        private AvatarService bookAvatarService;
        private AvatarService userAvatarService;

        public BookAdminController(ParserFactory parserFactory, IAuthorizationService authz, BookAvatarService bookAvatarService, UserAvatarService userAvatarService)
        {
            this.authz = authz;
            this.parserFactory = parserFactory;
            this.bookAvatarService = bookAvatarService;
            this.userAvatarService = userAvatarService;
            ViewData["SelectedTab"] = HeaderTabs.Admin;
        }

        public ActionResult Index()
        {
            List<Book> books = null;
            using (var context = new SideNotesEntities())
            {
                books = context.Books.Include("Avatar.Small").ToList();
            }
            return View(books);
        }

        [HttpPost]
        public ActionResult AddBook(HttpPostedFileBase ebookfile)
        {
            if (!authz.Authorize(Operation.AddBook)) throw new UnauthorizedAccessException("Нет прав на добавление книг.");
            string ext = Path.GetExtension(ebookfile.FileName);
            Stream inputStream = null;
            string tempFileName = null;
            if (ext == ".zip")
            {
                tempFileName = Unzip(ebookfile.InputStream);
                inputStream = System.IO.File.Open(tempFileName, FileMode.Open);
                ext = Path.GetExtension(tempFileName);
            }
            else
            {
                inputStream = ebookfile.InputStream;
            }

            ParserType parserType = ParserType.Unknown;
            if (ext == ".fb2")
            {
                parserType = ParserType.FB2Parser;
            }
            using (var parser = parserFactory.CreateParser(parserType))
            {
                parser.Parse(inputStream);
                inputStream.Close();
            }
            if (tempFileName != null)
            {
                System.IO.File.Delete(tempFileName);
            }
            return RedirectToAction("Index");
        }

        private string Unzip(Stream zippedFile)
        {
            var tempfolder = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TempFolder"]);
            var zippedFileName = tempfolder + DateTime.Now.ToString("YYYY-MM-DD-HH-mm-ss") + ".zip";
            var file = System.IO.File.Create(zippedFileName);
            zippedFile.CopyTo(file);
            file.Close();
            string resultFileName = null;
            using (ZipFile zip1 = ZipFile.Read(zippedFileName))
            {
                ZipEntry firstEntry = zip1.FirstOrDefault();
                if(firstEntry != null){
                    firstEntry.Extract(tempfolder, ExtractExistingFileAction.OverwriteSilently);
                    resultFileName = tempfolder + firstEntry.FileName;
                }
            }
            System.IO.File.Delete(zippedFileName);
            return resultFileName;
        }

        [HttpGet]
        public ActionResult EditBook(int Id)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == Id);
                if (book == null) throw new ArgumentException("Книга не найдена");
                if (!authz.Authorize(Operation.EditBook, book)) throw new UnauthorizedAccessException("Нет прав на редактирование книги.");

                var model = new EditBookModel();
                model.Id = book.Id;
                model.Title = book.Title;
                model.Annotation = book.Annotation;
                model.AuthorEmail = book.AuthorsEmail;
                model.HashTag = book.HashTag;
                model.DonationForm = book.DonationForm;
                model.DonationMessage = book.DonationMessage;
                model.PropertyStatus = book.PropertyStatus;
                model.Description = book.MetaDescription;
                model.Keywords = book.MetaKeywords;
                model.CustomStyles = book.CustomStyles;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult EditBook(EditBookModel model)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Неверный формат данных");
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.FirstOrDefault(b => b.Id == model.Id);
                if (book == null) throw new ArgumentException("Книга не найдена");
                if (!authz.Authorize(Operation.EditBook, book)) throw new UnauthorizedAccessException("Нет прав на редактирование книги.");
                PropertyStatus status = (PropertyStatus)model.PropertyStatus;
                book.Annotation = model.Annotation ?? "";
                book.AuthorsEmail = model.AuthorEmail;
                book.HashTag = model.HashTag;
                book.PropertyStatus = (int)status;
                book.DonationMessage = model.DonationMessage;
                book.DonationForm = model.DonationForm;
                book.CustomStyles = model.CustomStyles;
                book.MetaKeywords = model.Keywords;
                book.MetaDescription = model.Description;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult DeleteBook(int Id)
        {
            using(var context = new SideNotesEntities()){
                var book = context.Books.FirstOrDefault(b=> b.Id == Id);
                if (book == null) throw new ArgumentException("Книга не найдена");
                if (!authz.Authorize(Operation.DeleteBook, book)) throw new UnauthorizedAccessException("Нет прав на удаление книг.");
                context.DeleteBook(Id);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteChapter()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            return View();
        }

        [HttpPost]
        public ActionResult DeleteChapter(int Id)
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                if (context.Chapters.Any(c => c.ParentChapter_Id == Id)) 
                    throw new InvalidOperationException("Глава имеет вложенные главы. Нельзя удалить.");
                var chapter = context.Chapters.FirstOrDefault(c => c.Id == Id);
                if (chapter == null)
                    throw new InvalidOperationException("Глава не найдена");
                var latterChapters = context.Chapters.Where(c => c.OrderNumber > chapter.OrderNumber).ToList();
                latterChapters.ForEach(c => c.OrderNumber--);
                context.Chapters.DeleteObject(chapter);
                context.SaveChanges();
            }
            ViewBag.Message = "Глава успешно удалена";
            return View();
        }

        [HttpGet]
        public ActionResult DeleteParagraph()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            return View();
        }

        [HttpPost]
        public ActionResult DeleteParagraph(int Id)
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                if (context.HeadComments.Any(c => c.EntityType == (int)EntityType.Paragraph && c.EntityId == Id))
                    throw new InvalidOperationException("Абзац имеет комментарии. Нельзя удалить.");
                var paragraph = context.Paragraphs.FirstOrDefault(c => c.Id == Id);
                if (paragraph == null)
                    throw new InvalidOperationException("Абзац не найден");
                var latterParagraphs = context.Paragraphs.Where(p => p.OrderNumber > paragraph.OrderNumber).ToList();
                latterParagraphs.ForEach(c => c.OrderNumber--);
                context.Paragraphs.DeleteObject(paragraph);
                context.SaveChanges();
            }
            ViewBag.Message = "Абзац успешно удален";
            return View();
        }

        [HttpPost]
        public ActionResult SetAvatar(int id, HttpPostedFileBase file, bool? json)
        {
            Book book = null;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.First(u => u.Id == id);
            }
            if (book == null) throw new ArgumentException("Книга не найдена");
            if (!authz.Authorize(Operation.EditBook, book))
                throw new UnauthorizedAccessException("Недостаточно прав для редактирования книг");
            if (file != null)
            {
                bookAvatarService.UploadNew(id, file.InputStream);
            }
            if (json == true) return Json(new { });
            return RedirectToAction("Index");
        }

        public ActionResult UpdateChapterAll()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                var books = context.Books.ToList();
                foreach (var book in books)
                {
                    UpdateChapterIds(book.Id);
                }
            }
            return Content("OK");
        }

        public ActionResult UpdateChapterIds(int BookId)
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                var chaptersDic = (from chap in context.Chapters
                                join p in context.Paragraphs
                                    on chap.TopParagraph_Id equals p.Id
                                    where chap.Book_Id == BookId
                                select new { chapter = chap, paragraph = p }).ToList();
                foreach (var item in chaptersDic.OrderBy(i => i.chapter.OrderNumber))
                {
                    var paragraphs = context.Paragraphs
                        .Where(p => p.Book_Id == BookId 
                                && p.OrderNumber >= item.paragraph.OrderNumber).ToList();
                    paragraphs.ForEach(p => p.ChapterId = item.chapter.Id);
                }
                context.SaveChanges();
            }
            return Content("OK");
        }

        public ActionResult FixAvatarSize()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                var path = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TempFolder"]);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var tempFileName = path + "temp" + DateTime.Now.Ticks.ToString();

                var users = context.Users.Include("Avatar.Original").Where(u => u.Avatar != null).ToList();
                foreach (var user in users)
                {
                    System.IO.File.Copy(user.Avatar.Original.Location, tempFileName, true);
                    var stream = System.IO.File.Open(tempFileName, FileMode.Open);
                    userAvatarService.UploadNew(user.Id, stream);
                    stream.Close();
                }

                var books = context.Books.Include("Avatar.Original").Where(b => b.Avatar != null).ToList();
                foreach (var book in books)
                {
                    System.IO.File.Copy(book.Avatar.Original.Location, tempFileName, true);
                    var stream = System.IO.File.Open(tempFileName, FileMode.Open);
                    bookAvatarService.UploadNew(book.Id, stream);
                    stream.Close();
                }
                System.IO.File.Delete(tempFileName);
            }
            return RedirectToAction("Index", "BookAdmin");
        }

        public ActionResult FixPhotoDimensions()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                var fixer = new PhotoFixer();
                var sbError = new StringBuilder(1024);
                foreach (var photo in context.Photos.Where(p => p.width == 0 || p.height == 0))
                {
                    try
                    {
                        fixer.SetDimensions(photo);
                    }
                    catch (Exception ex)
                    {
                        sbError.Append(ex.Message + ";");
                    }
                }
                context.SaveChanges();
                if (sbError.Length > 0)
                {
                    throw new Exception(sbError.ToString());
                }
            }
            return RedirectToAction("Index", "BookAdmin");
        }

        public ActionResult FixInnerComments()
        {
            if (!authz.Authorize(Operation.EditBook)) throw new UnauthorizedAccessException("Нет прав на редактирование книг.");
            using (var context = new SideNotesEntities())
            {
                var comments = context.Comments.ToList();
                foreach (var comment in comments)
                {
                    var headComment = comment.HeadComment;
                    var head = new HeadComment()
                    {
                        Author_Id = comment.Author_Id,
                        EntityId = headComment.EntityId,
                        EntityType= headComment.EntityType,
                        Text = comment.Text,
                        DateCreated = comment.DateCreated
                    };
                    context.HeadComments.AddObject(head);
                    context.Comments.DeleteObject(comment);
                }
                context.SaveChanges();
            }
            return RedirectToAction("Index", "BookAdmin");
        }
    }
}
