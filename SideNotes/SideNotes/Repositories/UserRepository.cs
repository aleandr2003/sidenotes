using SideNotes.Extensions;
using SideNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Collections.Concurrent;
using SideNotes.ViewModels;

namespace SideNotes.Repositories
{
    public class UserRepository : IUserRepository
    {
        private static HashSet<string> forbiddenUrlNames;
        private static ConcurrentDictionary<string, User> urlNamesToUsers;

        static UserRepository()
        {
            forbiddenUrlNames = new HashSet<string>(GetForbiddenUrlNames());
            List<User> users = GetUsersWithUrlNames();
            urlNamesToUsers = new ConcurrentDictionary<string, User>(users.Select(u => new KeyValuePair<string, User>(u.UrlName, u)));
        }

        private static List<string> GetForbiddenUrlNames()
        {
            List<string> forbidden = new List<string>()
            {
                "resources",
                "scripts",
                "content"
            };

            Assembly asm = Assembly.GetExecutingAssembly();
            List<string> controllerNames = asm.GetControllerNames();
            List<string> forbiddenNames = controllerNames.Select(c => c.EndsWith("Controller") ? c.Substring(0, c.Length - "Controller".Length) : c)
                .Select(c => c.ToLower()).ToList();
            forbidden.AddRange(forbiddenNames);
            return forbidden;
        }

        public bool IsUrlNameAvailable(int userId, string urlName)
        {
            if (String.IsNullOrEmpty(urlName)) return true;
            string normalizedUrlName = urlName.ToLower();

            if (forbiddenUrlNames.Contains(normalizedUrlName))
                return false;

            using (var context = new SideNotesEntities())
            {
                return !context.Users.Any(u => u.UrlName == normalizedUrlName && u.Id != userId);
            }
        }

        public void UpdateSettings(int userId, EditSettingsModel settings)
        {
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                if (!String.IsNullOrEmpty(settings.UrlName) && !IsUrlNameAvailable(user.Id, settings.UrlName))
                {
                    throw new ArgumentException(Resources.User.UrlNameAlreadyInUse, "UrlName");
                }

                string oldUrlName = user.UrlName;
                user.UrlName = settings.UrlName;
                context.SaveChanges();

                urlNamesToUsers.TryRemove(oldUrlName, out var _);
                urlNamesToUsers.TryAdd(user.UrlName, user);
            }
        }

        private static List<User> GetUsersWithUrlNames()
        {
            using (var context = new SideNotesEntities())
            {
                return context.Users.Where(u => u.UrlName != null && u.UrlName != String.Empty).ToList();
            }
        }
    }
}