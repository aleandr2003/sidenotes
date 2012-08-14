using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Models;
using SideNotes.ViewModels;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class LayoutController : SidenotesController
    {
        private readonly IUserSession userSession;
        public LayoutController(IUserSession userSession)
        {
            this.userSession = userSession;
        }

        public ActionResult Header(int? tab)
        {
            ViewBag.SelectedTab = (HeaderTabs)(tab ?? (int)HeaderTabs.None);
            using (var context = new SideNotesEntities())
            {
                User user = null;
                if (userSession.IsAuthenticated)
                {
                    user = context.Users.Include("Avatar.Tiny")
                        .FirstOrDefault(u => u.Id == userSession.CurrentUser.Id);
                }
                
                return View("LayoutHeader", user);
            }
        }

    }
}
