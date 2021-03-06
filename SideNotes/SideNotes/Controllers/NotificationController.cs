﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.Services;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class NotificationController : SidenotesController
    {
        private INotificationSender sender;
        private IBookAuthorNotifier bookAuthorNotifier;
        public NotificationController(INotificationSender sender, IBookAuthorNotifier bookAuthorNotifier)
        {
            this.sender = sender;
            this.bookAuthorNotifier = bookAuthorNotifier;
        }

        public ActionResult SendAll()
        {
            if (!Request.ServerVariables["REMOTE_HOST"].StartsWith("10."))
                throw new UnauthorizedAccessException("Only server can access massmail actions");
            sender.SendAll();
            return View("OKView");
        }

        public ActionResult CreateAuthorsDigest()
        {
            if (!Request.ServerVariables["REMOTE_HOST"].StartsWith("10."))
                throw new UnauthorizedAccessException("Only server can access massmail actions");
            using (var context = new SideNotesEntities())
            {
                var bookIds = context.Books.Select(b => b.Id).ToList();
                foreach (var bookId in bookIds)
                {
                    bookAuthorNotifier.CreateDailyDigest(bookId);
                }
            }
            return View("OKView");
        }
    }
}
