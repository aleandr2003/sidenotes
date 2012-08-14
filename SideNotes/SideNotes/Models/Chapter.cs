using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public partial class Chapter
    {
        public List<Chapter> ChildChapters;
        public Paragraph TopParagraph;
    }
}