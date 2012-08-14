using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace SideNotes.Models
{
    public partial class HeadComment : IComment
    {
        int IComment.Id { get { return Id; } }
        string IComment.Text { get { return Text; } set { Text = value; } }
        User IComment.Author { get { return Author; } set { Author = value; } }
        DateTime IComment.DateCreated { get { return DateCreated; } set { DateCreated = value; } }

        IComment IComment.Parent { get { return null; } }
        int? IComment.Author_Id { get { return Author_Id; } }

        public int GetChildCommentsCount()
        {
            using (var context = new SideNotesEntities())
            {
                return context.Comments.Count(c => c.HeadCommentId == Id 
                    && 
                    c.Author_Id != null);
            }
        }

        public void BuildTree(List<Comment> childComments)
        {
            Hashtable hashTable = new Hashtable();
            foreach (var comment in childComments.Where(c => c.HeadCommentId == this.Id))
            {
                hashTable.Add(comment.Id, comment);
            }
            foreach (Comment comment in hashTable.Values)
            {
                if (comment.ParentCommentId == null)
                {
                    this.ChildComments.Add(comment);
                }
                else
                {
                    var parent = (Comment)hashTable[comment.ParentCommentId];
                    parent.ChildComments.Add(comment);
                }
            }
        }

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
    }
}