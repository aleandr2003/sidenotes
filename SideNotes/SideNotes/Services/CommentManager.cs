using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Transactions;
using SideNotes.Services.Templates;

namespace SideNotes.Services
{
    public class CommentManager : ICommentManager
    {
        ICommentNotifier notifier;
        public CommentManager(ICommentNotifier notifier)
        {
            this.notifier = notifier;
        }

        public int SaveTemporaryComment(int entityId, int entityType, string commentText, bool isPrivate)
        {
            using (var context = new SideNotesEntities())
            {
                TempAnonymousComment newComment = new TempAnonymousComment()
                {
                    Text = commentText,
                    EntityId = entityId,
                    EntityType = entityType,
                    isPrivate = isPrivate
                };
                context.TempAnonymousComments.AddObject(newComment);
                context.SaveChanges();
                return newComment.Id;
            }
        }

        public void PublishTemporaryComment(int tempId, int authorId)
        {
            using (var context = new SideNotesEntities())
            {
                using (var scope = new TransactionScope())
                {
                    TempAnonymousComment tempComment = context.TempAnonymousComments.FirstOrDefault(c => c.Id == tempId);
                    if (tempComment == null) throw new InvalidOperationException(Resources.ErrorMessages.TemporaryCommentNotFound);

                    AddHeadComment(context, authorId, tempComment.EntityId, tempComment.EntityType, tempComment.Text, tempComment.isPrivate);
                    context.DeleteObject(tempComment);
                    context.SaveChanges();
                    scope.Complete();
                }
            }
        }

        public void AddHeadComment(int AuthorId, int entityId, int entityType, string commentText, bool isPrivate)
        {
            using (var context = new SideNotesEntities())
            {
                AddHeadComment(context, AuthorId, entityId, entityType, commentText, isPrivate);
            }
        }

        private void AddHeadComment(SideNotesEntities context, int AuthorId, int entityId, int entityType, string commentText, bool isPrivate)
        {
            HeadComment newComment = new HeadComment()
            {
                Text = commentText,
                DateCreated = DateTime.Now,
                Author_Id = AuthorId,
                EntityId = entityId,
                EntityType = entityType,
                IsPrivate = isPrivate
            };
            context.HeadComments.AddObject(newComment);
            context.SaveChanges();
            
            try
            {
                notifier.NotifyNewHeadComment(newComment, GetAbsoluteUrl());
            }
            catch {
                //TODO log error
            }
        }

        

        public void AddComment(int AuthorId, int? parentCommentId, int headCommentId, string commentText)
        {
            int paragraphId = 0;
            var comment = Comment.GetParent(parentCommentId, headCommentId);
            if (comment == null) throw new InvalidOperationException(Resources.ErrorMessages.CommentNotFound);
            if (comment.IsDeleted) throw new InvalidOperationException(Resources.ErrorMessages.CommentRemoved);
            using (var context = new SideNotesEntities())
            {
                paragraphId = context.HeadComments.First(c => c.Id == headCommentId).EntityId;
                Comment newComment = new Comment()
                {
                    ParentCommentId = parentCommentId,
                    Text = commentText,
                    DateCreated = DateTime.Now,
                    Author_Id = AuthorId,
                    HeadCommentId = headCommentId
                };
                context.Comments.AddObject(newComment);
                context.SaveChanges();

                try
                {
                    notifier.NotifyNewComment(newComment, GetAbsoluteUrl());
                }
                catch
                {
                    //TODO log error
                }
            }
        }

        private string GetAbsoluteUrl()
        {
            return $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}";
        }
    }
}