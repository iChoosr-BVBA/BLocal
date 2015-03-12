using System;
using System.Linq;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.History;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HistoryController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public HistoryController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var allValues = localization.ValueManager.GetAllValuesQualified();
            localization.HistoryManager.AdjustHistory(allValues, Session.Get<String>("author"));
            var history = localization.HistoryManager.ProvideHistory()
                .OrderByDescending(h => h.LatestEntry().DateTimeUtc)
                .ToList();

            var model = new HistoryData {
                Provider = localization,
                History = history
            };
            return View(model);
        }


        public ActionResult Fix(String providerConfigName)
        {
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = localization.ValueManager.GetAllValuesQualified();
            localization.HistoryManager.AdjustHistory(allValues, "History fix (" + Session.Get<String>("author") + ")");
            localization.HistoryManager.Persist();
            return RedirectToAction("Index", new { providerConfigName });
        }
    }
}
