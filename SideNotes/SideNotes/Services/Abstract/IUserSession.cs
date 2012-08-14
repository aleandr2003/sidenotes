using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;

namespace SideNotes.Services.Abstract
{
    public interface IUserSession
    {
        void LogIn(User user);
        void LogOut();
        User CurrentUser { get; }
        int? UserId { get; }
        Sharer Sharer { get; set; }
        bool IsAuthenticated { get; }
        User Authenticate(string login, string password);
    }
}