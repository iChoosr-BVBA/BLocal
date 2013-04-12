using System.Web.Mvc;

namespace BLocal.Web.Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Demo()
        {
            return View();
        }
    }
}