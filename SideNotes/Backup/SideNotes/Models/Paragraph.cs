using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

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
        private string noteId;
        private string title;
        private string note;

        public override FormatType FormatType
        {
            get
            {
                return FormatType.FootNote;
            }
        }

        public string GetNoteId()
        {
            if (String.IsNullOrEmpty(noteId)) parseContent();
            return noteId;
        }

        public string GetNote()
        {
            if (String.IsNullOrEmpty(note)) parseContent();
            return note;
        }
        public string GetTitle()
        {
            if (String.IsNullOrEmpty(title)) parseContent();
            return title;
        }

        private void parseContent()
        {
            var xcontent = XElement.Parse(Content);
            var titleElement = xcontent.Elements().Where(e =>
                e.Attributes("class").Any() &&
                e.Attribute("class").Value == "footNoteTitle").FirstOrDefault();
            if (titleElement != null)
            {
                if (titleElement.Attributes("noteId").Any())
                {
                    noteId = titleElement.Attribute("noteId").Value;
                }
                title = titleElement.Value;
            }
            titleElement.Remove();
            note = xcontent.Value;
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