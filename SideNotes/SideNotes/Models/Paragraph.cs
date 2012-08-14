using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public partial class Paragraph
    {
        public virtual FormatType FormatType
        {
            get
            {
                return FormatType.Common;
            }
        }
        public int GetChildCommentsCount(int? CurrentUserId)
        {
            using (var context = new SideNotesEntities())
            {
                return context.HeadComments.Where(
                    c => c.EntityId == Id
                        && c.EntityType == (int)EntityType.Paragraph
                        && (!c.IsPrivate || c.Author_Id == CurrentUserId)).ToList()
                        .Select(c => c.GetChildCommentsCount() + (c.Author_Id == null ? 0 : 1)) // + 1 - чтобы учесть сам HeadComment
                        .Sum();
            }
        }
        public bool IsCommentable
        {
            get
            {
                return !(this is EmptyLine);
            }
        }
    }

    public partial class Title : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.Title;
            }
        }
    }

    public partial class Cite : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.Cite;
            }
        }
    }

    public partial class CiteAuthor : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.CiteAuthor;
            }
        }
    }

    public partial class Epigraph : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.Epigraph;
            }
        }
    }

    public partial class EpigraphAuthor : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.EpigraphAuthor;
            }
        }
    }

    public partial class Poem : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.Poem;
            }
        }
    }

    public partial class PoemAuthor : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.PoemAuthor;
            }
        }
    }

    public partial class EmptyLine : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.EmptyLine;
            }
        }
    }

    public partial class FootNote : Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.FootNote;
            }
        }
    }

    public enum FormatType
    {
        Common = 0,
        Title,
        Epigraph,
        EpigraphAuthor,
        Cite,
        CiteAuthor,
        Poem,
        PoemAuthor,
        EmptyLine,
        Image,
        FootNote
    }
}