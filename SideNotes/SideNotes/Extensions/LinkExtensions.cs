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
        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, bool requireAbsoluteUrl)
        {
            return htmlHelper.ActionLink(linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary(), requireAbsoluteUrl);
        }

        public static string ActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool requireAbsoluteUrl)
        {
            if (!requireAbsoluteUrl)
                return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes).ToString();

            HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
            RouteData routeData = RouteTable.Routes.GetRouteData(currentContext);

            routeData.Values["controller"] = controllerName;
            routeData.Values["action"] = actionName;

            if (routeData.Route is DomainRoute domainRoute)
            {
                DomainData domainData = domainRoute.GetDomainData(new RequestContext(currentContext, routeData), routeData.Values);
                return htmlHelper.ActionLink(linkText, actionName, controllerName, domainData.Protocol, domainData.HostName, domainData.Fragment, routeData.Values, null).ToString();
            }

            return htmlHelper.ActionLink(linkText, actionName, controllerName, routeValues, htmlAttributes).ToString();
        }
    }
}