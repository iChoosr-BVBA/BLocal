using System;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.History;
using BLocal.Web.Manager.Providers.RemoteAccess;

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

            var history = localization.HistoryManager.ProvideHistory()
                .OrderByDescending(h => h.LatestEntry().DateTimeUtc)
                .ToList();

            var historyChecker = new HistoryChecker();
            if(historyChecker.FindValuesConflictingWithHistory(allValues, history).Any())
                return RedirectToAction("Broken", new { providerConfigName = providerConfigName });

            var model = new HistoryData {
                Provider = localization,
                History = history
            };
            return View(model);
        }


        public ActionResult Fix(String providerConfigName)
        {
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = localization.ValueManager.GetAllValuesQualified().ToArray();
            var history = localization.HistoryManager.ProvideHistory();
            var historyChecker = new HistoryChecker();
            var historyConflicts = historyChecker.FindValuesConflictingWithHistory(allValues, history).ToArray();

            if(historyConflicts.Length > 3 && localization.HistoryManager is RemoteAccessManager)
                ((RemoteAccessManager) localization.HistoryManager).StartBatch();

            foreach(var conflict in historyConflicts)
                localization.HistoryManager.ProgressHistory(new QualifiedValue(conflict.Qualifier, conflict.CurrentValue), Session.Get<String>("author"));

            localization.HistoryManager.Persist();
            
            if(historyConflicts.Length > 3 && localization.HistoryManager is RemoteAccessManager)
                ((RemoteAccessManager) localization.HistoryManager).EndBatch();

            return RedirectToAction("Index", new { providerConfigName });
        }

        public ActionResult Broken(String providerConfigName)
        {
            var provider = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = provider.ValueManager.GetAllValuesQualified().ToArray();
            var history = provider.HistoryManager.ProvideHistory();
            var historyChecker = new HistoryChecker();
            var historyConflicts = historyChecker.FindValuesConflictingWithHistory(allValues, history).ToList();
            return View(new BrokenHistoryData { ConflictingValues = historyConflicts, Provider = provider });
        }
    }
}
