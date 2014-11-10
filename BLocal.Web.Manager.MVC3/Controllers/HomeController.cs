using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.Home;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HomeController : Controller
    {
        private const string TranslationProviderGroupName = "translationProviderGroup";

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

        [HttpPost]
        public ActionResult LoadTranslations(String providerConfigName)
        {
            Session.Set(TranslationProviderGroupName, ProviderGroupFactory.CreateProviderGroup(providerConfigName));
            return RedirectToAction("VerifyTranslation");
        }

        public ActionResult VerifyTranslation()
        {
            var localization = Session.Get<ProviderGroup>(TranslationProviderGroupName);
            if (localization == null)
                return RedirectToAction("Overview");

            var allValues = localization.ValueManager.GetAllValuesQualified().ToArray();

            var groupedTranslations = allValues
                .ToLookup(v => new Qualifier(v.Qualifier.Part, TranslationVerificationData.NoLocale, v.Qualifier.Key));

            var allLocales = allValues
                .Select(v => v.Qualifier.Locale)
                .Distinct()
                .ToArray();

            return View(new TranslationVerificationData(groupedTranslations, allLocales));
        }

        [ValidateInput(false)]
        public JsonResult TransUpdate(String part, String locale, String key, String value)
        {
            var localization = Session.Get<ProviderGroup>(TranslationProviderGroupName);
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.UpdateCreateValue(new QualifiedValue(qualifier, value));

            localization.ValueManager.Persist();
            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult TransDelete(String part, String key)
        {
            var localization = Session.Get<ProviderGroup>(TranslationProviderGroupName);
            if (localization == null)
                throw new Exception("Localization not loaded!");

            localization.ValueManager.DeleteLocalizationsFor(Part.Parse(part), key);

            localization.ValueManager.Persist();
            return Json(new { ok = true });
        }
    }
}
