using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BLocal.Web.Manager.Context
{
    public class AuthenticateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var route = filterContext.RouteData.Values;
            if (new[]{"index", "authenticate"}.Contains(route["action"].ToString().ToLowerInvariant()) && route["controller"].ToString().ToLowerInvariant() == "home")
                return;

            var lastAuth = filterContext.HttpContext.Session["auth"] as DateTime?;

            if (lastAuth == null)
                Block(filterContext);
            else
                filterContext.HttpContext.Session["auth"] = DateTime.Now;
        }

        private static void Block(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new HttpStatusCodeResult((int) HttpStatusCode.Forbidden, "please authenticate");
                return;
            }

            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    {"action", "Index"},
                    {"controller", "Home"},
                    {"blockedRequest", filterContext.HttpContext.Request.RawUrl}
                }
            );
        }
    }
}