using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;

namespace SideNotes.Extensions
{
    public static class UrlHelperExtensions
    {
        private const string SiteUrl = "http://sidenotes.ru";
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
    }
}