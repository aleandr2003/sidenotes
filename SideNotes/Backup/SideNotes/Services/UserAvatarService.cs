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
    public class UserAvatarService : AvatarService
    {
        public static string NoAvatarTiny { get; private set; }
        public static string NoAvatarSmall { get; private set; }
        public static string NoAvatarLarge { get; private set; }

        static UserAvatarService()
        {
            NoAvatarTiny = VirtualPathUtility.ToAbsolute("~/Content/img/no_avatar_tiny.gif");
            NoAvatarSmall = VirtualPathUtility.ToAbsolute("~/Content/img/no_avatar_small.gif");
            NoAvatarLarge = VirtualPathUtility.ToAbsolute("~/Content/img/no_avatar_large.gif");
        }

        public override void UploadNew(int Id, Stream avatarStream)
        {
            var avatarImg = new Bitmap(avatarStream);

            var tinySized = CropSquareImage(avatarImg, 16);
            var smallSized = CropSquareImage(avatarImg, 42);
            var largeSized = ResizeImage(avatarImg, width: 200);
            
            User user = null;
            using (var context = new SideNotesEntities())
            {
                user = context.Users.Include("Avatar").First(u => u.Id == Id);
            }
            if (user.Avatar != null)
            {
                RemoveAvatar(Id);
            }
            using (var context = new SideNotesEntities())
            {
                user = context.Users.First(u => u.Id == Id);
                string filename = string.Format("{0}_({1})", user.Name, user.Id);
                var tiny = imageKeeper.Upload(tinySized, filename + "_tiny");
                var small = imageKeeper.Upload(smallSized, filename + "_small");
                var large = imageKeeper.Upload(largeSized, filename + "_large");
                var original = imageKeeper.Upload(avatarImg, filename + "_orig");
                
                var Avatar = new Avatar
                {
                    Tiny = tiny,
                    Small = small,
                    Large = large,
                    Original = original
                };
                Avatar.AllPhotos.ToList().ForEach(p => context.Photos.AddObject(p));
                context.Avatars.AddObject(Avatar);
                user.Avatar = Avatar;
                context.SaveChanges();
            }
        }

        private void RemoveAvatar(int userId)
        {
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.First(u => u.Id == userId);
            
                var avatar = user.Avatar;
                base.RemoveAvatar(avatar);
                user.Avatar = null;
                List<Photo> photos = avatar.AllPhotos.Where(p => p != null).ToList();
                context.Avatars.DeleteObject(avatar);
                foreach (Photo photo in photos)
                {
                    context.Photos.DeleteObject(photo);
                }
                context.SaveChanges();
            }
        }

        public UserAvatarService(IImageKeeper imageKeeper)
            : base(imageKeeper)
        {
        }

    }
}