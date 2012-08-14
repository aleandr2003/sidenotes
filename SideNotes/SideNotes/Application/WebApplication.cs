using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace SideNotes.Application
{
    public class WebApplication : SideNotesApplication
    {
        protected override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "CommentsByUser", // Route name
                "Book/CommentsByUser/{UserId}/{BookId}", // URL with parameters
                new { controller = "Book", action = "CommentsByUser" } // Parameter defaults
            );
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            
        }
    }
}