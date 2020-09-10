using SideNotes.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SideNotes.Controllers.Abstract
{
    public abstract class SidenotesController : Controller
    {
        protected static string isChecked = "on";
    }
}
