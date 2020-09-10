﻿using SideNotes.Application;
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
            if (controllerName.ToLower() == "book")
            {
                var customRoute = RouteTable.Routes.Where(r => r is BookRoute).Select(r => r as BookRoute).FirstOrDefault();
                routeData = customRoute?.GetBookRouteData(currentContext);
            }
            if (routeData == null)
            {
                routeData = RouteTable.Routes.GetRouteData(currentContext);
            }

            if (routeData != null)
            {
                routeData.Values["controller"] = controllerName;
                routeData.Values["action"] = actionName;

                if (routeData.Route is BookRoute bookRoute)
                {
                    BookData bookData = bookRoute.GetBookData(new RequestContext(currentContext, routeData), routeValues);
                    if (bookData != null)
                    {
                        routeData.Values.Remove("id");
                        return htmlHelper.ActionLink(linkText, actionName, bookData.Annotator, bookData.Protocol, bookData.HostName, null, routeData.Values, null);
                    }
                }

                if (routeData.Route is DomainRoute domainRoute)
                {
                    DomainData domainData = domainRoute.GetDomainData(new RequestContext(currentContext, routeData), routeData.Values);
                    return htmlHelper.ActionLink(linkText, actionName, controllerName, domainData.Protocol, domainData.HostName, domainData.Fragment, routeData.Values, null);
                }
            }
            return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes);
        }
    }
}