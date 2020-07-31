using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using SideNotes.Repositories;

namespace SideNotes.Application
{
    public class WebApplication : SideNotesApplication
    {
        protected override void RegisterRoutes(RouteCollection routes)
        {
            BookRoute.Init(new BookRepository());

            routes.Add("BookRoute", new BookRoute(
                "{booktitle}.annotated.by",     // Domain with parameters
                "{annotator}/{action}",    // URL with parameters
                new { booktitle = "", action = "Start", annotator = "" }  // Parameter defaults
            ));

            routes.Add("BookRouteDefault1", new BookRoute(
                "{booktitle}.annotated.by",     // Domain with parameters
                "{annotator}",    // URL with parameters
                new { booktitle = "", action = "Start", annotator = "" }  // Parameter defaults
            ));

            routes.Add("BookRouteDefault2", new BookRoute(
                "{booktitle}.annotated.by",     // Domain with parameters
                "{annotator}/",    // URL with parameters
                new { booktitle = "", action = "Start", annotator = "" }  // Parameter defaults
            ));

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