using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using System.IO;
using System.Drawing;
using SideNotes.Models;

namespace SideNotes.Services
{
    public class BookAvatarService : AvatarService
    {
        public static string NoAvatarSmall { get; private set; }
        public static string NoAvatarMedium { get; private set; }
        public static string NoAvatarLarge { get; private set; }

        static BookAvatarService()
        {
            NoAvatarSmall = VirtualPathUtility.ToAbsolute("~/Content/img/no_bookavatar_small.jpg");
            NoAvatarMedium = VirtualPathUtility.ToAbsolute("~/Content/img/no_bookavatar_medium.jpg");
            NoAvatarLarge = VirtualPathUtility.ToAbsolute("~/Content/img/no_bookavatar_large.jpg");
        }

        public override void UploadNew(int Id, Stream avatarStream)
        {
            var avatarImg = new Bitmap(avatarStream);

            var smallSized = ResizeImage(avatarImg, width: 65);
            var mediumSized = ResizeImageHeight(avatarImg, 210);
            var largeSized = ResizeImage(avatarImg, width: 200);
            Book book = null;
            using (var context = new SideNotesEntities())
            {
                book = context.Books.Include("Avatar").First(u => u.Id == Id);
            }
            if (book.Avatar != null)
            {
                RemoveAvatar(Id);
            }
            using (var context = new SideNotesEntities())
            {
                book = context.Books.First(u => u.Id == Id);
                string filename = string.Format("{0}_({1})", book.Title, book.Id);
                var small = imageKeeper.Upload(smallSized, filename + "_small");
                var medium = imageKeeper.Upload(mediumSized, filename + "_medium");
                var large = imageKeeper.Upload(largeSized, filename + "_large");
                var original = imageKeeper.Upload(avatarImg, filename + "_orig");
                
                var Avatar = new Avatar
                {
                    Small = small,
                    Medium = medium,
                    Large = large,
                    Original = original
                };
                Avatar.AllPhotos.ToList().ForEach(p => context.Photos.AddObject(p));
                context.Avatars.AddObject(Avatar);
                book.Avatar = Avatar;
                context.SaveChanges();
            }
        }

        private void RemoveAvatar(int bookId)
        {
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.First(u => u.Id == bookId);

                var avatar = book.Avatar;
                base.RemoveAvatar(avatar);
                book.Avatar = null;
                List<Photo> photos = avatar.AllPhotos.Where(p => p != null).ToList();
                context.Avatars.DeleteObject(avatar);
                foreach (Photo photo in photos)
                {
                    context.Photos.DeleteObject(photo);
                }
                context.SaveChanges();
            }
        }

        public BookAvatarService(IImageKeeper imageKeeper)
            : base(imageKeeper)
        {
        }

    }
}