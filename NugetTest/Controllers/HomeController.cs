using System.Web.Mvc;
using BLocal.Core;
using BLocal.Providers;

namespace NugetTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            const string cs = "Data Source=backseat.eu;Initial Catalog=Localizationtest;persist security info=True;user id=DevBeta;password=\"Dev-n-Beta123\";";
            var logAndValueProvider = new MSDBValueAndLogProvider(cs, "Parts2", "Languages2", "LocalizedValues2", "LocalizedLogs2", "dbo", true);
            var localeProvider = new  ConstantLocaleProvider(new Locale("en"));
            var partProvider = new ConstantPartProvider(Part.Parse("NugetTest"));
            var notifier = new ExceptionNotifier();
            var repository = new LocalizationRepository(logAndValueProvider, notifier, localeProvider, partProvider);
            return View(repository.Get("test"));
        }
    }
}