using SideNotes.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace SideNotes.Extensions
{
    public static class LinkExtensions
    {
        public static MvcHtmlString ActionLink<T>(this HtmlHelper<T> htmlHelper, string linkText, string actionName, string controllerName, bool requireAbsoluteUrl)
        {
            return htmlHelper.ActionLink(linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static MvcHtmlString ActionLink<T>(this HtmlHelper<T> htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes, bool requireAbsoluteUrl)
        {
            return htmlHelper.ActionLink(linkText, actionName, controllerName, new RouteValueDictionary(routeValues), new RouteValueDictionary(htmlAttributes), requireAbsoluteUrl);
        }

        public static MvcHtmlString ActionLink<T>(this HtmlHelper<T> htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool requireAbsoluteUrl)
        {
            if (!requireAbsoluteUrl)
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);

            HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
            RouteData routeData = null;
            if (controllerName.ToLower() == "book" && routeValues.ContainsKey("id"))
            {
                var customRoute = RouteTable.Routes.Where(r => r is BookRoute).Select(r => r as BookRoute).FirstOrDefault();
                BookData bookData = customRoute.GetBookData(currentContext, routeValues);
                if (bookData != null)
                {
                    routeValues.Remove("id");
                    if (String.IsNullOrEmpty(bookData.Annotator))
                    {
                        return htmlHelper.ActionLink(linkText, String.Empty, actionName, bookData.Protocol, bookData.HostName, null, routeValues, htmlAttributes);
                    }
                    return htmlHelper.ActionLink(linkText, actionName, bookData.Annotator, bookData.Protocol, bookData.HostName, null, routeValues, htmlAttributes);
                }
            }

            routeData = RouteTable.Routes.GetRouteData(currentContext);
            if (routeData != null)
            {
                routeData.Values["controller"] = controllerName;
                routeData.Values["action"] = actionName;

                if (routeData.Route is DomainRoute domainRoute)
                {
                    DomainData domainData = domainRoute.GetDomainData(new RequestContext(currentContext, routeData), routeData.Values);
                    return htmlHelper.ActionLink(linkText, actionName, controllerName, domainData.Protocol, domainData.HostName, domainData.Fragment, routeData.Values, htmlAttributes);
                }
            }
            return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
        }
    }
}