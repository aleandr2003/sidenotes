using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Web.Mvc;
using SideNotes.Extensions;
using SideNotes.Services.Templates;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;
using SideNotes.Exceptions;

namespace SideNotes.Services
{
    public class CommentNotifier : ICommentNotifier
    {
        ITemplateLoader templateLoader;
        string culture;
        private BufferBlock<Tuple<IComment, string>> newCommentsQueue = new BufferBlock<Tuple<IComment, string>>();

        public CommentNotifier(ITemplateLoader templateLoader)
        {
            this.templateLoader = templateLoader;
            this.culture = System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag.ToLower();
            Task.Run(async() => { await ProcessQueue(); });
        }

        public void NotifyNewComment(Comment newComment, string absoluteUrl)
        {
            newCommentsQueue.Post(new Tuple<IComment, string>(newComment, absoluteUrl));
        }

        public void NotifyNewHeadComment(HeadComment newComment, string absoluteUrl)
        {
            newCommentsQueue.Post(new Tuple<IComment, string>(newComment, absoluteUrl));
        }

        private void CreateNewCommentNotification(Comment newComment, string absoluteUrl) { 
            IComment parentComment = Comment.GetParent(newComment.ParentCommentId, newComment.HeadCommentId);
            using (var context = new SideNotesEntities())
            {
                User receiver = context.Users.FirstOrDefault(u => u.Id == parentComment.Author_Id);
                User author = context.Users.FirstOrDefault(u => u.Id == newComment.Author_Id);
                HeadComment headComment = context.HeadComments.FirstOrDefault(c => c.Id == newComment.HeadCommentId);

                if (receiver == null) throw new InvalidOperationException(Resources.Comment.UserNotFound);
                if (receiver.NotifyAuthorCommentReplied && !String.IsNullOrEmpty(receiver.Email) && receiver.Id != author.Id)
                {
                    string discussionLink = "";
                    if (headComment.EntityType == (int)EntityType.Paragraph)
                    {
                        discussionLink = $"{absoluteUrl}/Book/CommentsThread?headCommentId={headComment.Id}";
                    }

                    EmailTemplate template = this.templateLoader.GetEmailTemplate("NewCommentTemplate", culture);
                    if (template == null)
                        throw new TemplateNotFoundException("Failed to load template");
                    string body = String.Format(template.Body,
                            receiver.Name,
                            HttpUtility.HtmlEncode(author.Name),
                            $"{absoluteUrl}/User/View/{newComment.Author_Id}",
                            HttpUtility.HtmlEncode(newComment.Text),
                            discussionLink
                        );
                    Notification newNotification = new Notification()
                    {
                        Subject = template.Subject,
                        Email = receiver.Email,
                        Body = body
                    };
                    context.Notifications.AddObject(newNotification);
                    context.SaveChanges();
                }
            }
        }

        private void CreateNewHeadCommentNotification(HeadComment newComment, string absoluteUrl)
        {
            using (var context = new SideNotesEntities())
            {
                var otherComments = context.HeadComments.Where(
                    h => h.EntityId == newComment.EntityId 
                    && h.EntityType == newComment.EntityType
                    && h.Author_Id != null).ToList();
                var AuthorIds = otherComments.Where(c => c.Author_Id != newComment.Author_Id).Select(c => c.Author_Id).Distinct().ToList();

                EmailTemplate template = this.templateLoader.GetEmailTemplate("NewHeadCommentTemplate", culture);
                if (template == null)
                    throw new TemplateNotFoundException("Failed to load template");

                foreach (int AuthorId in AuthorIds)
                {
                    User receiver = context.Users.FirstOrDefault(u => u.Id == AuthorId);
                    User author = context.Users.FirstOrDefault(u => u.Id == newComment.Author_Id);

                    if (receiver == null) throw new InvalidOperationException(Resources.Comment.UserNotFound);
                    if (receiver.NotifyAuthorCommentReplied && !String.IsNullOrEmpty(receiver.Email) && receiver.Id != author.Id)
                    {
                        string readerLink = "";
                        if (newComment.EntityType == (int)EntityType.Paragraph)
                        {
                            var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == newComment.EntityId);
                            readerLink = $"{absoluteUrl}/Book/View/{paragraph.Book_Id}?skip={paragraph.OrderNumber - 1}&expanded=on";
                            var book = context.Books.First(b => b.Id == paragraph.Book_Id);
                            string body = String.Format(template.Body,
                                    receiver.Name,
                                    HttpUtility.HtmlEncode(author.Name),
                                    $"{absoluteUrl}/User/View/{newComment.Author_Id}",
                                    book.Title,
                                    $"{absoluteUrl}/Book/Start/{book.Id}",
                                    HttpUtility.HtmlEncode(newComment.Text),
                                    readerLink
                                );
                            Notification newNotification = new Notification()
                            {
                                Subject = template.Subject,
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

        private async Task ProcessQueue()
        {
            while(true)
            {
                try
                {
                    var (comment, absoluteUrl) = await newCommentsQueue.ReceiveAsync();
                    switch (comment)
                    {
                        case Comment c:
                            CreateNewCommentNotification(c, absoluteUrl);
                            break;
                        case HeadComment hc:
                            CreateNewHeadCommentNotification(hc, absoluteUrl);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //TODO write logs
                }
            }
        }
    }
}