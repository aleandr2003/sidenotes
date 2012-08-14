using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public interface IComment
    {
        int Id { get; }
        string Text { get; set; }
        User Author { get; set; }
        int? Author_Id { get; }
        DateTime DateCreated { get; set; }

        List<IComment> ChildComments { get; }
        IComment Parent { get;}

        bool IsDeleted { get; }
    }
}