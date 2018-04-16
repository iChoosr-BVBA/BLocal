using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Ganss.XSS;
using iChoosr.Sanitizer;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;

namespace BLocal.Web.Manager
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
#if DEBUG
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            InitializeSanitizer();
        }

        protected void InitializeSanitizer()
        {
            var configFilePath = Server.MapPath("sanitizerconfiguration.json");
            var configurationContent = File.ReadAllText(configFilePath);
            var configuration = JsonConvert.DeserializeObject<SanitizerConfiguration>(configurationContent);
            var sanitizer = new Sanitizer(new HtmlSanitizer(), configuration);
            Application.Add("sanitizer", sanitizer);
        }
    }
}