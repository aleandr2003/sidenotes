using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using System.Drawing.Drawing2D;
using System.Drawing;
using SideNotes.Models;
using System.IO;

namespace SideNotes.Services.Abstract
{
    public abstract class AvatarService
    {
        public abstract void UploadNew(int Id, Stream avatarStream);

        public void RemoveAvatar(Avatar avatar)
        {
            foreach (Photo photo in avatar.AllPhotos.Where(p => !p.Equals(default(Photo))))
            {
                imageKeeper.Delete(photo);
            }
        }

        public AvatarService(IImageKeeper imageKeeper)
        {
            this.imageKeeper = imageKeeper;
        }

        protected static Bitmap ResizeImage(Image imgToResize, int width)
        {
            return ResizeImageWidth(imgToResize, width);
        }

        protected static Bitmap ResizeImageWidth(Image imgToResize, int width)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            var nPercent = (float)width / sourceWidth;
            int destHeight = (int)(sourceHeight * nPercent);

            var b = new Bitmap(width, destHeight);
            using (var g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, width, destHeight);
            }

            return b;
        }

        protected static Bitmap ResizeImageHeight(Image imgToResize, int height)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            var nPercent = (float)height / sourceHeight;
            int destWidth = (int)(sourceWidth * nPercent);

            var b = new Bitmap(destWidth, height);
            using (var g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, destWidth, height);
            }

            return b;
        }

        protected static Bitmap CropSquareImage(Image imgToResize, int side)
        {
            Bitmap source = null;
            if (imgToResize.Width < imgToResize.Height)
            {
                source = ResizeImageWidth(imgToResize, side);
            }
            else
            {
                source = ResizeImageHeight(imgToResize, side);
            }

            Rectangle cropRect = new Rectangle(0, 0, side, side);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using(Graphics g = Graphics.FromImage(target))
            {
               g.DrawImage(source, new Rectangle(0, 0, target.Width, target.Height), 
                                cropRect,                        
                                GraphicsUnit.Pixel);
            }
            return target;
        }

        protected readonly IImageKeeper imageKeeper;
    }
}