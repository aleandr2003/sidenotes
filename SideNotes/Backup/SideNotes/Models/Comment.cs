using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public partial class Comment :IComment
    {
        int IComment.Id{ get { return Id; } }
        string IComment.Text { get { return Text; } set { Text = value; } }
        User IComment.Author { get { return Author; } set { Author = value; } }
        DateTime IComment.DateCreated { get { return DateCreated; } set { DateCreated = value; } }
        IComment IComment.Parent { get { return HeadComment; } }
        int? IComment.Author_Id { get { return Author_Id; } }

        private List<IComment> childComments;
        public List<IComment> ChildComments
        {
            get
            {
                if (childComments == null) childComments = new List<IComment>();
                return childComments;
            }
        }

        public bool IsDeleted
        {
            get
            {
                return Author_Id == null;
            }
        }

         public static IComment GetParent(int? parentCommentId, int headCommentId)
        {
            using (var context = new SideNotesEntities())
            {
                if (parentCommentId != null) return context.Comments.FirstOrDefault(c => c.Id == parentCommentId);
                else return context.HeadComments.FirstOrDefault(c => c.Id == headCommentId);
            }
        }
    }

    public enum EntityType
    {
        Paragraph,
        Comment,
        HeadComment
    }
}