using System.Web;
using System.Web.Routing;
using BLocal.Core;

namespace BLocal.Web
{
    public class MvcPartProvider : IPartProvider
    {
        private readonly Part _defaultPart;

        public MvcPartProvider(Part defaultPart)
        {
            _defaultPart = defaultPart;
        }

        public Part GetCurrentPart()
        {
            var route = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current));
            return route == null
                ? _defaultPart
                : new Part((route.Values["Action"] ?? _defaultPart.Name).ToString(), new Part(route.Values["Controller"].ToString()));
        }
    }
}
