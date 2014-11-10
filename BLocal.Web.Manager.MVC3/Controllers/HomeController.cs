using System;
using System.Configuration;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HomeController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public HomeController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Overview()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Authenticate(String username, String password)
        {
            if (password == ConfigurationManager.AppSettings["password"])
                Session["auth"] = DateTime.Now;
            Session["author"] = username;
            return RedirectToAction("Overview");
        }
    }
}
