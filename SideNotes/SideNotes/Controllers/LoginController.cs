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
    public class LoginController : SidenotesController
    {
        readonly IUserSession userSession;

        public LoginController(IUserSession userSession)
        {
            this.userSession = userSession;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult Index(string ErrorMessage, string returnUrl)
        {
            if (userSession.IsAuthenticated)
            {
                return RedirectToAction("View", "User", new { id = userSession.CurrentUser.Id });
            }
            ViewBag.ErrorMessage = ErrorMessage;
            ViewBag.returnUrl = returnUrl;
            return View(new LoginModel());
        }

        [HttpPost]
        public ActionResult LogIn(LoginModel form, string returnUrl, bool? json)
        {
            if (String.IsNullOrEmpty(returnUrl)) returnUrl = Url.Action("Index", "Home");
            User currentUser = null;
            try
            {
                if (!ModelState.IsValid) throw new ArgumentException(Resources.Login.ControllerInvalidFormat);
                currentUser = userSession.Authenticate(form.Email, form.Password ?? "");
            }
            catch (ArgumentException ex)
            {
                if (json == true) return Json(new { ErrorMessage = ex.Message });
                return RedirectToAction("Index", "Login", new { ErrorMessage = ex.Message });
            }
            using (var context = new SideNotesEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == currentUser.Id);
                user.LastLoginSource = AccountSource.SideNotes;
                context.SaveChanges();
            }
            userSession.LogIn(currentUser);
            if (json == true) return Json(new { RedirectUrl = returnUrl });
            return Redirect(returnUrl);
        }

        [HttpPost]
        public ActionResult LogOut()
        {
            userSession.LogOut();
            return RedirectToAction("Index","Home");
        }

    }
}
