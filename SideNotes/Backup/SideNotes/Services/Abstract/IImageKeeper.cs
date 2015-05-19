using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;
using System.Drawing;

namespace SideNotes.Services.Abstract
{
    public interface IImageKeeper
    {
        Photo Upload(Bitmap bitmap, string name);
        void Delete(Photo photo);
    }
}