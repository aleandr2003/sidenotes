using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;

namespace SideNotes.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public bool Authorize(Operation op, object obj)
        {
            switch (op)
            {
                case Operation.EditUser:
                    {
                        var u = obj as User;
                        return curUser != null && u != null && u.Id == curUser.Id;
                    }
                case Operation.RemoveBookmark:
                    {
                        var b = obj as Bookmark;
                        return curUser != null && b.Owner_Id == curUser.Id;
                    }
                case Operation.DeleteBook:
                    {
                        return curUser != null && curUser.IsAdmin;
                    }
                case Operation.EditBook:
                    {
                        return curUser != null && curUser.IsAdmin;
                    }
               
                case Operation.DeleteComment:
                    {
                        var c = obj as Comment;
                        return curUser != null && (curUser.IsAdmin || c.Author_Id == curUser.Id);
                    }
                case Operation.DeleteHeadComment:
                    {
                        var c = obj as HeadComment;
                        return curUser != null && (curUser.IsAdmin || c.Author_Id == curUser.Id);
                    }
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        public bool Authorize(Operation op)
        {
            switch (op)
            {
                case Operation.AddBook:
                    return curUser != null && curUser.IsAdmin;
                case Operation.EditBook:
                    {
                        return curUser != null && curUser.IsAdmin;
                    }
                case Operation.EditCatalog:
                    return curUser != null && curUser.IsAdmin;
                case Operation.ManageUsers:
                    return curUser != null && curUser.IsAdmin;
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        public AuthorizationService(IUserSession userSession)
        {
            curUser = userSession.IsAuthenticated ? userSession.CurrentUser : null;
        }

        private readonly User curUser;
    }

    public enum Operation
    {
        EditUser,
        RemoveBookmark,
        AddBook,
        DeleteBook,
        EditBook,
        DeleteComment,
        DeleteHeadComment,
        EditCatalog,
        ManageUsers
    }
}