using System;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.TranslationVerification;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class TranslationVerificationController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public TranslationVerificationController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var allValues = localization.ValueManager.GetAllValuesQualified().ToArray();

            var groupedTranslations = allValues
                .ToLookup(v => new Qualifier(v.Qualifier.Part, TranslationVerificationData.NoLocale, v.Qualifier.Key));

            var allLocales = allValues
                .Select(v => v.Qualifier.Locale)
                .Distinct()
                .ToArray();

            return View(new TranslationVerificationData(groupedTranslations, allLocales, localization));
        }

        [ValidateInput(false)]
        public JsonResult Update(String part, String locale, String key, String value, String providerConfigName)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            var qualifiedValue = new QualifiedValue(qualifier, value);
            providerGroup.ValueManager.UpdateCreateValue(qualifiedValue);
            providerGroup.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));

            providerGroup.ValueManager.Persist();
            if (providerGroup.ValueManager != providerGroup.HistoryManager)
                providerGroup.HistoryManager.Persist();

            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult Delete(String part, String key, String providerConfigName)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var localizationsToDelete = providerGroup.ValueManager.GetAllValuesQualified().Where(v => v.Qualifier.Part.ToString() == part).ToArray();

            foreach (var localization in localizationsToDelete)
            {
                providerGroup.ValueManager.DeleteValue(localization.Qualifier);
                providerGroup.HistoryManager.ProgressHistory(new QualifiedValue(localization.Qualifier, null), Session.Get<String>("author"));
            }

            providerGroup.ValueManager.Persist();
            if (providerGroup.ValueManager != providerGroup.HistoryManager)
                providerGroup.HistoryManager.Persist();

            return Json(new { ok = true });
        }
    }
}
