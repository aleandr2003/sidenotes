﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using System.Web.Security;

namespace SideNotes.Services
{
    public class UserSession : IUserSession
    {
        public UserSession()
        {
            var login = HttpContext.Current.User.Identity.Name;
            using (var context = new SideNotesEntities())
            {
                CurrentUser = context.Users.Where(u => u.Login == login).FirstOrDefault();
            }
            IsAuthenticated = CurrentUser != null;
            if (IsAuthenticated)
            {
                Sharer = SharerFactory.GetSharer(CurrentUser);
            }
        }

        public void LogIn(User user)
        {
            CurrentUser = user;
            FormsAuthentication.SetAuthCookie(user.Login, true);
        }

        public void LogOut()
        {
            FormsAuthentication.SignOut();
            Sharer = null;
        }

        public User CurrentUser
        {
            get;
            private set;
        }

        public int? UserId
        {
            get
            {
                return IsAuthenticated ? (int?)CurrentUser.Id : null;
            }
        }

        public Sharer Sharer{ get; set; }

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public User Authenticate(string login, string password)
        {
            User user;
            using (var context = new SideNotesEntities())
            {
                user = context.Users.Where(u => u.Login == login).FirstOrDefault();
            }
            if (user != null)
            {
                if (user.AccountSource == AccountSource.SideNotes && user.PasswordMatches(password)) return user;
                if (user.AccountSource == AccountSource.Facebook)
                    throw new ArgumentException(Resources.ErrorMessages.PleaseLoginWithFacebook);
                if (user.AccountSource == AccountSource.Twitter)
                    throw new ArgumentException(Resources.ErrorMessages.PleaseLoginWithTwitter);
            }

            throw new ArgumentException(Resources.ErrorMessages.WrongLoginPasswordPair);
        }

    }
}