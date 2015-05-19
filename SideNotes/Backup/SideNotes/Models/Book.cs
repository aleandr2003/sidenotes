using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models.Helpers;
using SideNotes.Extensions;

namespace SideNotes.Models
{
    public partial class Book
    {
        public int GetCommentatorsCount(int? CurrentUserId)
        {
            using (var context = new SideNotesEntities())
            {
                return (from p in context.Paragraphs
                        join h in context.HeadComments.Where(
                             c => c.EntityType == (int)EntityType.Paragraph
                             && (!c.IsPrivate || c.Author_Id == CurrentUserId)
                             && c.Author_Id != null)
                         on p.Id equals h.EntityId
                        join u in context.Users
                         on h.Author_Id equals u.Id
                        where p.Book_Id == Id
                        select u).Distinct().Count();
            }
        }

        public int GetCommentsCount(int? CurrentUserId)
        {
            using (var context = new SideNotesEntities())
            {
                return (from p in context.Paragraphs
                        join h in context.HeadComments.Where(
                             c => c.EntityType == (int)EntityType.Paragraph
                             && (!c.IsPrivate || c.Author_Id == CurrentUserId)
                             && c.Author_Id != null)
                         on p.Id equals h.EntityId
                        where p.Book_Id == Id
                        select h).Count();
            }
        }

        public int GetCommentsCountByAuthor(int AuthorId, int? CurrentUserId)
        {
            using (var context = new SideNotesEntities())
            {
                return (from p in context.Paragraphs
                        join h in context.HeadComments.Where(
                             c => c.EntityType == (int)EntityType.Paragraph
                             && (!c.IsPrivate || c.Author_Id == CurrentUserId)
                             && c.Author_Id == AuthorId)
                         on p.Id equals h.EntityId
                        where p.Book_Id == Id
                        select h).Count();
            }
        }

        public HeadComment GetBestComment()
        {
            using (var context = new SideNotesEntities())
            {
                var headCommentsDic = (from p in context.Paragraphs
                                       join h in context.HeadComments
                                        on p.Id equals h.EntityId
                                       join c in context.Comments
                                        on h.Id equals c.HeadCommentId
                                       where p.Book_Id == Id
                                       && h.EntityType == (int)EntityType.Paragraph
                                       && h.Author_Id != null
                                       && !h.IsPrivate
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
                    var commentsList = (from p in context.Paragraphs
                                   join h in context.HeadComments
                                    on p.Id equals h.EntityId
                                   where p.Book_Id == Id
                                   && h.EntityType == (int)EntityType.Paragraph
                                   && h.Author_Id != null
                                   && !h.IsPrivate
                                   select h).ToList();
                    if(commentsList.Any()){
                        var randomIndex = (new Random()).Next(commentsList.Count);
                        bestComment = commentsList[randomIndex];
                    }
                }
                return bestComment;
            }
        }
    }

    public enum PropertyStatus
    {
        Public,
        Donatable,
        Private
    }
}