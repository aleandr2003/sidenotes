using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models.Helpers;

namespace SideNotes.Models
{
    public partial class User
    {
        private Password _password;
        private Password Password
        {
            get
            {
                if (_password == null)
                {
                    _password = new Password(PasswordHash, PasswordSeed);
                }
                return _password;
            }
        }

        public void SetPassword(string newPassword, string oldPassword)
        {
            if (PasswordHash!=null && PasswordSeed != null && !Password.Matches(oldPassword)) 
                throw new ArgumentException(Resources.User.ControllerCurrentPasswordDoesntMatch);
            var pass = new Password(newPassword);
            PasswordHash = pass.Hash;
            PasswordSeed = pass.Salt;
        }

        public void SetPassword(string newPassword)
        {
            SetPassword(newPassword, null);
        }

        public bool PasswordMatches(string input)
        {
            return Password.Matches(input);
        }

        public AccountSource AccountSource
        {
            get
            {
                return (AccountSource)AccountSourceInt;
            }
            set
            {
                AccountSourceInt = (int)value;
            }
        }

        public AccountSource LastLoginSource
        {
            get
            {
                return (AccountSource)LastLoginSourceInt;
            }
            set
            {
                LastLoginSourceInt = (int)value;
            }
        }

        

        public bool IsMyFriend(int HisId)
        {
            bool isFriend = false;
            using (var context = new SideNotesEntities())
            {
                //Эта конструкция выглядит совершенно поидиотски, 
                //потому что EF не поддерживает вызов пользовательских функций
                //на самом деле мне просто нужно вызвать IsMyFriend в базе данных
                isFriend = (from u in context.Users
                            where u.Id == Id
                           select EntitiesHelper.IsMyFriend(HisId, Id)).First();
            }

            return isFriend;
        }

        public void SetForeignAccountId(AccountInfo info)
        {
            switch (info.AccountSource)
            {
                case AccountSource.Facebook:
                    FacebookId = info.Id;
                    break;
                case AccountSource.Twitter:
                    TwitterId = info.Id;
                    break;
                default: throw new NotSupportedException(Resources.Misc.NotSupportedAccountType);
            }
        }

        public int CountBooksRead()
        {
            using (var context = new SideNotesEntities())
            {
                return context.Autosaves.Count(a => a.Owner_Id == Id);
            }
        }
        public bool HasComments()
        {
            using (var context = new SideNotesEntities())
            {
                return context.HeadComments.Any(c => c.Author_Id == Id && !c.IsPrivate);
            }
        }

        public int CommentsCount()
        {
            using (var context = new SideNotesEntities())
            {
                return context.HeadComments.Count(c => c.Author_Id == Id && !c.IsPrivate);
            }
        }
    }
}