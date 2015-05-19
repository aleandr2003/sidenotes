using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Controllers.Abstract;

namespace SideNotes.Controllers
{
    public class ErrorController : SidenotesController
    {
        //
        // GET: /Error/

        public ActionResult Index(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }

    }
}
