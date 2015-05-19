using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.IO;
using System.Drawing;

namespace SideNotes.Services
{
    public class PhotoFixer
    {
        public void SetDimensions(Photo photo)
        {
            var stream = File.Open(photo.Location, FileMode.Open);
            var bitmap = new Bitmap(stream);
            photo.width = bitmap.Width;
            photo.height = bitmap.Height;
            stream.Close();
        }
    }
}