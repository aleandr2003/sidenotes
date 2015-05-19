using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Drawing;
using System.Web.Hosting;
using System.Drawing.Imaging;
using System.IO;

namespace SideNotes.Services
{
    public class ImageKeeper:IImageKeeper
    {
        private readonly string _path;
        private readonly string _relativePath;

        public ImageKeeper(string relativePath)
        {
            _path = HostingEnvironment.MapPath(relativePath);
            _relativePath = relativePath;
        }

        public Photo Upload(Bitmap bitmap, string name)
        {
            name = CleanName(name);
            string fullname = _path + name + ".jpg";
            string relativeName = _relativePath + name + ".jpg";
            bitmap.Save(fullname, ImageFormat.Jpeg);
            return new Photo() { Location = fullname, Url = relativeName, width = bitmap.Width, height = bitmap.Height };
        }

        private string CleanName(string name)
        {
            return name.Replace(":", " ").Replace("?", "").Replace("№", "").Replace("/", "");
        }

        public void Delete(Photo photo)
        {
            if (File.Exists(photo.Location))
            {
                File.Delete(photo.Location);
            }
        }

    }
}