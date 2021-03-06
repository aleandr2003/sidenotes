﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Services.Abstract;
using SideNotes.Models;

namespace SideNotes.Services
{
    public class SharerFactory
    {
        public static Sharer GetSharer(User user)
        {
            switch (user.LastLoginSource)
            {
                case AccountSource.Facebook:
                    return user.AccessTokenFacebook != null ? new FacebookSharer(user.AccessTokenFacebook, user) : null;
                case AccountSource.Twitter:
                    return user.AccessTokenTwitter != null ? new TwitterSharer(user.AccessTokenTwitter, user) : null;
                case AccountSource.SideNotes:
                    return null;
                default:
                    throw new NotImplementedException("Unsupported sharer type");
            }
        }
    }
}