using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SideNotes.Models;

namespace SideNotes.ViewModels
{
    public class NotificationSettingsModel
    {
        public bool NotifyAuthorCommentReplied { get; set; }

        public NotificationSettingsModel()
        { }

        public NotificationSettingsModel(User user)
        {
            NotifyAuthorCommentReplied = user.NotifyAuthorCommentReplied;
        }
    }
}