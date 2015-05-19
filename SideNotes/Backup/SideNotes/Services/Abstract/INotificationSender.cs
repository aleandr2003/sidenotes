using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.Services.Abstract
{
    public interface INotificationSender
    {
        void SendAll();
    }
}