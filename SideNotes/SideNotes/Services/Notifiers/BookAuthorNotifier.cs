using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Text;
using SideNotes.Extensions;
using System.Web.Mvc;
using SideNotes.Services.Templates;

namespace SideNotes.Services
{
    public class BookAuthorNotifier : IBookAuthorNotifier
    {
        ITemplateLoader templateLoader;
        string culture;

        public BookAuthorNotifier(ITemplateLoader templateLoader)
        {
            this.templateLoader = templateLoader;
            this.culture = System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag.ToLower();
        }

        public void CreateDailyDigest(int bookId)
        {
            Book book;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.FirstOrDefault(b => b.Id == bookId);
                if (book == null) throw new ArgumentException(Resources.ErrorMessages.BookNotFound);
            }

            if (String.IsNullOrEmpty(book.AuthorsEmail)) return;

            UrlHelper urlHelper = UrlHelperExtensions.GetUrlHelper();
            StringBuilder commentRows = new StringBuilder(1024);
            var styleOdd = "background-color:#FFC;";
            var styleEven = "background-color:#FFF;";
            EmailTemplate commentRowTemplate = this.templateLoader.GetEmailTemplate("CommentRowTemplate", culture);
            EmailTemplate dailyCommentDigestTemplate = this.templateLoader.GetEmailTemplate("DailyCommentDigestTemplate", culture);

            using (var context = new SideNotesEntities())
            {
                var previousDate = DateTime.Now.AddDays(-1).Date;
                var currentDate = DateTime.Now.Date;
                var comments = (from p in context.Paragraphs
                                join h in context.HeadComments.Where(h => h.EntityType == (int)EntityType.Paragraph)
                                 on p.Id equals h.EntityId
                                where p.Book_Id == bookId
                                && h.Author_Id != null
                                && !h.IsPrivate
                                && h.DateCreated > previousDate
                                && h.DateCreated < currentDate
                                orderby h.DateCreated descending
                                select h).ToList();
                if (comments.Count == 0) return;
                var rowCount = 0;
                foreach(var comment in comments){
                    rowCount++;
                    commentRows.AppendLine(
                        String.Format(
                            commentRowTemplate.Body,
                            comment.Author.Name,
                            urlHelper.ActionAbsolute("View", "User", new {Id = comment.Author_Id}),
                            comment.Text,
                            urlHelper.ActionAbsolute("Index", "Feed", new { Id = bookId, paragraphId = comment.EntityId }),
                            comment.DateCreated.ToString("(hh:mm)"),
                            rowCount % 2 == 0 ? styleEven : styleOdd
                            ));
                }
                string date = DateTime.Now.AddDays(-1).ToString("dd MMM yyyy");
                string body = String.Format(dailyCommentDigestTemplate.Body, 
                    commentRows.ToString(),
                    date
                );
                Notification newNotification = new Notification()
                {
                    Subject = String.Format(dailyCommentDigestTemplate.Subject, book.Title , date),
                    Email = book.AuthorsEmail,
                    Body = body
                };
                context.Notifications.AddObject(newNotification);
                context.SaveChanges();
            }
        }
    }
}