﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Models.Queries
{
    public class GetForeignAccountQuery
    {
        private AccountInfo info;
        public GetForeignAccountQuery(AccountInfo info)
        {
            this.info = info;
        }

        public User Load(SideNotesEntities context)
        {
            if (info.Email != null)
            {
                var user = context.Users.FirstOrDefault(u => u.Email == info.Email);
                if (user != null) return user;
            }
            switch (info.AccountSource)
            {
                case AccountSource.Facebook:
                    return context.Users.FirstOrDefault(u => u.FacebookId == info.Id);
                case AccountSource.Twitter:
                    return context.Users.FirstOrDefault(u => u.TwitterId == info.Id);
                default: throw new NotSupportedException(Resources.Misc.NotSupportedAccountType);
            }
        }
    }
}