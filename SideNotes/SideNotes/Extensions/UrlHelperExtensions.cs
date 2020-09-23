using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using SideNotes.Application;

namespace SideNotes.Extensions
{
    public static class UrlHelperExtensions
    {
        private const string SiteUrl = "http://annotated.by";
        private static UrlHelper urlHelper;
        public static Uri GetBaseUrl(this UrlHelper url)
        {
            Uri contextUri = new Uri(url.RequestContext.HttpContext.Request.Url, url.RequestContext.HttpContext.Request.RawUrl);
            UriBuilder realmUri = new UriBuilder(contextUri) { Path = url.RequestContext.HttpContext.Request.ApplicationPath, Query = null, Fragment = null };
            return realmUri.Uri;
        }

        public static Uri ActionAbsolute(this UrlHelper url, string actionName, string controllerName)
        {
            return new Uri(GetBaseUrl(url), url.Action(actionName, controllerName));
        }

        public static Uri ActionAbsolute(this UrlHelper url, string actionName)
        {
            return new Uri(GetBaseUrl(url), url.Action(actionName));
        }

        public static Uri ActionAbsolute(this UrlHelper url, string actionName, object actionArgs)
        {
            return new Uri(GetBaseUrl(url), url.Action(actionName, actionArgs));
        }

        public static Uri ActionAbsolute(this UrlHelper url, string actionName, string controllerName, object actionArgs)
        {
            return new Uri(GetBaseUrl(url), url.Action(actionName, controllerName, actionArgs));
        }
        public static Uri Absolute(this UrlHelper url, string relativeUrl)
        {
            return new Uri(GetBaseUrl(url), relativeUrl);
        }
        public static UrlHelper GetUrlHelper()
        {
            if (urlHelper != null) return urlHelper;
            var httpContext = HttpContext.Current;

            if (httpContext == null)
            {
                var request = new HttpRequest("/", SiteUrl, "");
                var response = new HttpResponse(new StringWriter());
                httpContext = new HttpContext(request, response);
            }

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = RouteTable.Routes.GetRouteData(httpContextBase);
            var requestContext = new RequestContext(httpContextBase, routeData);
            urlHelper = new UrlHelper(requestContext);
            return urlHelper;
        }

        public static string Action(this UrlHelper urlHelper, string actionName, string controllerName, bool requireAbsoluteUrl)
        {
            return urlHelper.Action(actionName, controllerName, new RouteValueDictionary(), requireAbsoluteUrl);
        }
        public static string Action(this UrlHelper urlHelper, string actionName, string controllerName, object routeValues, bool requireAbsoluteUrl)
        {
            return Action(urlHelper, actionName, controllerName, new RouteValueDictionary(routeValues), requireAbsoluteUrl);
        }

        public static string Action(this UrlHelper urlHelper, string actionName, string controllerName, RouteValueDictionary routeValues, bool requireAbsoluteUrl)
        {
            if (!requireAbsoluteUrl)
                return urlHelper.Action(actionName, controllerName, routeValues);

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
                        return urlHelper.Action(String.Empty, actionName, routeValues, bookData.Protocol, bookData.HostName);
                    }
                    return urlHelper.Action(actionName, bookData.Annotator, routeValues, bookData.Protocol, bookData.HostName);
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
                    return urlHelper.Action(actionName, controllerName, routeData.Values, domainData.Protocol, domainData.HostName);
                }
            }
            return urlHelper.Action(actionName, controllerName, routeValues);
        }
    }
}