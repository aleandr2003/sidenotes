using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;

namespace SideNotes.Services.Abstract
{
    public abstract class Sharer
    {
        protected string accessToken;
        protected User currentUser;

        public Sharer(string AccessToken, User user)
        {
            accessToken = AccessToken;
            currentUser = user;
        }

        public abstract void ShareComment(Paragraph paragraph, HeadComment comment);
        public abstract void ShareBook(Book book);
    }
}