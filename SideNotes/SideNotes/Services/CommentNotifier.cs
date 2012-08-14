using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Web.Mvc;
using SideNotes.Extensions;

namespace SideNotes.Services
{
    public class CommentNotifier
    {
        private const string NewCommentTemplate = @"Уважаемый {0}.<br/> 
Пользователь <a href='{2}'>{1}</a> ответил на ваш комментарий:<br/>
<br/>
{3}<br/>
<br/>
<a href='{4}'>Прочитать обсуждение</a>
";

        private const string NewHeadCommentTemplate = @"Уважаемый {0}.<br/> 
Пользователь <a href='{2}'>{1}</a> продолжил дискуссию в книге <a href='{4}'>'{3}'</a>:<br/>
<br/>
{5}<br/>
<br/>
<a href='{6}'>Прочитать обсуждение</a>
";

        public void NotifyNewComment(Comment newComment)
        {
            IComment parentComment = Comment.GetParent(newComment.ParentCommentId, newComment.HeadCommentId);
            using (var context = new SideNotesEntities())
            {
                User receiver = context.Users.FirstOrDefault(u => u.Id == parentComment.Author_Id);
                User author = context.Users.FirstOrDefault(u => u.Id == newComment.Author_Id);
                HeadComment headComment = context.HeadComments.FirstOrDefault(c => c.Id == newComment.HeadCommentId);

                if (receiver == null) throw new InvalidOperationException("Пользователь не найден");
                if (receiver.NotifyAuthorCommentReplied && !String.IsNullOrEmpty(receiver.Email) && receiver.Id != author.Id)
                {
                    UrlHelper urlHelper = UrlHelperExtensions.GetUrlHelper();
                    string discussionLink = "";
                    if (headComment.EntityType == (int)EntityType.Paragraph)
                    {
                        discussionLink = urlHelper.ActionAbsolute("CommentsThread", "Book", new { headCommentId = headComment.Id }).AbsoluteUri;
                    }
                    string body = String.Format(NewCommentTemplate,
                            receiver.Name,
                            HttpUtility.HtmlEncode(author.Name),
                            urlHelper.ActionAbsolute("View", "User", new { Id = newComment.Author_Id }).AbsoluteUri,
                            HttpUtility.HtmlEncode(newComment.Text),
                            discussionLink
                        );
                    Notification newNotification = new Notification()
                    {
                        Subject = "На ваш комментарий ответили",
                        Email = receiver.Email,
                        Body = body
                    };
                    context.Notifications.AddObject(newNotification);
                    context.SaveChanges();
                }
            }
        }
        public void NotifyNewHeadComment(HeadComment newComment)
        {
            using (var context = new SideNotesEntities())
            {
                var otherComments = context.HeadComments.Where(
                    h => h.EntityId == newComment.EntityId 
                    && h.EntityType == newComment.EntityType
                    && h.Author_Id != null).ToList();
                var AuthorIds = otherComments.Where(c => c.Author_Id != newComment.Author_Id).Select(c => c.Author_Id).Distinct().ToList();
                
                foreach (int AuthorId in AuthorIds)
                {
                    User receiver = context.Users.FirstOrDefault(u => u.Id == AuthorId);
                    User author = context.Users.FirstOrDefault(u => u.Id == newComment.Author_Id);

                    if (receiver == null) throw new InvalidOperationException("Пользователь не найден");
                    if (receiver.NotifyAuthorCommentReplied && !String.IsNullOrEmpty(receiver.Email) && receiver.Id != author.Id)
                    {
                        UrlHelper urlHelper = UrlHelperExtensions.GetUrlHelper();
                        string readerLink = "";
                        if (newComment.EntityType == (int)EntityType.Paragraph)
                        {
                            var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == newComment.EntityId);
                            readerLink = urlHelper.ActionAbsolute("View", "Book", new { Id = paragraph.Book_Id, skip = paragraph.OrderNumber - 1, expanded = "on" }).AbsoluteUri;
                            var book = context.Books.First(b => b.Id == paragraph.Book_Id);
                            string body = String.Format(NewHeadCommentTemplate,
                                    receiver.Name,
                                    HttpUtility.HtmlEncode(author.Name),
                                    urlHelper.ActionAbsolute("View", "User", new { Id = newComment.Author_Id }).AbsoluteUri,
                                    book.Title,
                                    urlHelper.ActionAbsolute("Start", "Book", new { Id = book.Id }),
                                    HttpUtility.HtmlEncode(newComment.Text),
                                    readerLink
                                );
                            Notification newNotification = new Notification()
                            {
                                Subject = "На ваш комментарий ответили",
                                Email = receiver.Email,
                                Body = body
                            };
                            context.Notifications.AddObject(newNotification);
                        }
                    }
                }
                context.SaveChanges();
            }
        }
    }
}