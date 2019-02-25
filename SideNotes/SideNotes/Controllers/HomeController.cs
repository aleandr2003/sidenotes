using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services;
using SideNotes.Models;
using SideNotes.Services.Abstract;
using SideNotes.Models.Queries;
using SideNotes.ViewModels;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class HomeController : SidenotesController
    {
        int topBooksCount = 3;
        private IUserSession userSession;
        public HomeController(IUserSession userSession)
        {
            this.userSession = userSession;
            ViewData["SelectedTab"] = HeaderTabs.Home;
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            List<Book> topBooks= null;
            int totalBooksCount = 0;
            List<int> ownedBookIds = new List<int>();
            using (var context = new SideNotesEntities())
            {
                int topPopularity = context.Books.Max(b => b.Popularity);
                topBooks = context.Books.Include("Avatar.Large")
                    .Where(b => b.Popularity == topPopularity)
                    .Take(topBooksCount).ToList();
                totalBooksCount = context.Books.Count();
                if (userSession.IsAuthenticated)
                {
                    ownedBookIds = new GetOwnedBookIdsQuery(userSession.CurrentUser.Id).Load(context);
                }
            }
            ViewBag.HasMore = totalBooksCount > topBooksCount;
            return View(Tuple.Create(topBooks, ownedBookIds));
        }

        public ActionResult Exception()
        {
            throw new Exception("test exception");
        }

        //public ActionResult PostText()
        //{
        //    try
        //    {
        //        if (userSession.Sharer == null) return Content("no sharer");
        //        if (!(userSession.Sharer is VkontakteSharer)) return Content("not vkontakte");

        //        using (var context = new SideNotesEntities())
        //        {
        //            var paragraph = context.Paragraphs.FirstOrDefault(p => p.Id == 1135);
        //            var comment = context.HeadComments.FirstOrDefault(h => h.Id == 21);
        //            return Content(((VkontakteSharer)userSession.Sharer).test(paragraph, comment));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //}

        public ActionResult ForAuthors()
        {
            ViewData["SelectedTab"] = HeaderTabs.Authors;
            return View();
        }
    }
}
