using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models
{
    public partial class ImageParagraph:Paragraph
    {
        public override FormatType FormatType
        {
            get
            {
                return FormatType.Image;
            }
        }
        public string Url
        {
            get
            {
                var start = Content.IndexOf("src=\"") + 5;
                var length = Content.IndexOf("\"", start) - start;
                return Content.Substring(start, length);
            }
        }
        public int Width
        {
            get
            {
                var start = Content.IndexOf("width=\"") + 7;
                var length = Content.IndexOf("\"", start) - start;
                int width = 0;
                Int32.TryParse(Content.Substring(start, length), out width);
                return width;
            }
        }
        public int Height
        {
            get
            {
                var start = Content.IndexOf("height=\"") + 8;
                var length = Content.IndexOf("\"", start) - start;
                int height = 0;
                Int32.TryParse(Content.Substring(start, length), out height);
                return height;
            }
        }
    }
}