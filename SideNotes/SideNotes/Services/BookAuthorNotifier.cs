using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Text;
using SideNotes.Extensions;
using System.Web.Mvc;

namespace SideNotes.Services
{
    public class BookAuthorNotifier
    {
        private const string DailyCommentDigestTemplate = @" 
{1} читатели сказали о вашей книге следующее:
<table>{0}
</table>

С Уважением 'Заметки на полях'.
";

        private const string CommentRowTemplate = @" 
<tr {5}>
<td>{4}</td>
<td><a href='{1}'>{0}</a> сказал:</td>
<td style='width:300px;'>{2}</td>
<td><a href='{3}'>Прочитать обсуждение</a></td>
</tr>
";
        private int bookId;
        private Book book;
        public BookAuthorNotifier(int bookId)
        {
            this.bookId = bookId;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.FirstOrDefault(b => b.Id == bookId);
                if (book == null) throw new ArgumentException("Книга не найдена");
            }
        }

        public void CreateDailyDigest(){
            if (String.IsNullOrEmpty(book.AuthorsEmail)) return;
            UrlHelper urlHelper = UrlHelperExtensions.GetUrlHelper();
            StringBuilder commentRows = new StringBuilder(1024);
            var styleOdd = "style=\"background-color:#FFC; \"";
            var styleEven = "style=\"background-color:#FFF; \"";
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
                            CommentRowTemplate,
                            comment.Author.Name,
                            urlHelper.ActionAbsolute("View", "User", new {Id = comment.Author_Id}),
                            comment.Text,
                            urlHelper.ActionAbsolute("Index", "Feed", new { Id = bookId, paragraphId = comment.EntityId }),
                            comment.DateCreated.ToString("(hh:mm)"),
                            rowCount % 2 == 0 ? styleEven : styleOdd
                            ));
                }
                string date = DateTime.Now.AddDays(-1).ToString("dd MMM yyyy");
                string body = String.Format(DailyCommentDigestTemplate, 
                    commentRows.ToString(),
                    date
                );
                Notification newNotification = new Notification()
                {
                    Subject = String.Format("Комментарии к книге '{0}' за {1}", book.Title , date),
                    Email = book.AuthorsEmail,
                    Body = body
                };
                context.Notifications.AddObject(newNotification);
                context.SaveChanges();
            }
        }
    }
}