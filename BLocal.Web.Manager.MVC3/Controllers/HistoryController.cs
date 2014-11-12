using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HistoryController : Controller
    {
        private const string HistoryProviderGroupName = "historyProviderGroup";

        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public HistoryController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var localization = Session.Get<ProviderGroup>(HistoryProviderGroupName);
            if (localization == null || localization.Name != providerConfigName)
                Session.Set(HistoryProviderGroupName, localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName));

            localization.HistoryManager.Reload();
            var allValues = localization.ValueManager.GetAllValuesQualified();
            localization.HistoryManager.AdjustHistory(allValues, Session.Get<String>("author"));
            var history = localization.HistoryManager.ProvideHistory()
                .OrderByDescending(h => h.LatestEntry().DateTime)
                .ToList();

            return View(history);
        }
    }
}
