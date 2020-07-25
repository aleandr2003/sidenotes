using SideNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Repositories
{
    public class UserRepository : IUserRepository
    {
        public bool IsUrlNameAvailable(int userId, string urlName)
        {
            if (String.IsNullOrEmpty(urlName)) return true;
            using (var context = new SideNotesEntities())
            {
                return !context.Users.Any(u => u.UrlName == urlName && u.Id != userId);
            }
        }
    }
}