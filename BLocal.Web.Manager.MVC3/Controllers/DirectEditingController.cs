using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.DirectEditing;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class DirectEditingController : Controller
    {
        private const string ManualProviderGroupName = "manualProviderGroup";

        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public DirectEditingController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var localization = Session.Get<ProviderGroup>(ManualProviderGroupName);
            if (localization == null || localization.Name != providerConfigName)
                Session.Set(ManualProviderGroupName, localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName));

            localization.ValueManager.Reload();
            localization.HistoryManager.Reload();
            
            var localizations = localization.ValueManager.GetAllValuesQualified().ToList();
            localization.HistoryManager.AdjustHistory(localizations, Session.Get<String>("author"));
            var history = localization.HistoryManager.ProvideHistory().ToDictionary(h => h.Qualifier);

            var groupedParts = localizations
                .Select(l => new QualifiedLocalization(l.Qualifier, l.Value, history[l.Qualifier]))
                .GroupBy(ql => ql.Qualifier.Part)
                .ToDictionary(@group => @group.Key, @group => new LocalizedPart(@group.Key, @group.ToList()));
            
            // make sure all branches are in the list
            foreach (var kvp in groupedParts.ToList())
                FixTree(groupedParts, kvp.Key);

            // get new list of all branches
            var nodesWithParents = groupedParts.Where(part => part.Key.Parent != null).ToList();

            //add branches to their parents
            foreach (var kvp in nodesWithParents)
                groupedParts[kvp.Key.Parent].Subparts.Add(kvp.Value);

            // remove branches from the list, keeping only root nodes
            foreach (var kvp in nodesWithParents)
                groupedParts.Remove(kvp.Key);

            localization.ValueManager.Persist();
            return View(groupedParts.Values);
        }

        public ActionResult ReloadLocalization(String providerConfigName)
        {
            var localization = Session.Get<ProviderGroup>(ManualProviderGroupName);
            if (localization == null)
                return RedirectToAction("Index", "Home");

            localization.ValueManager.Reload();
            return RedirectToAction("Index", new { providerConfigName });
        }

        [ValidateInput(false)]
        public JsonResult UpdateCreateValue(String part, String locale, String key, String content)
        {
            var localization = Session.Get<ProviderGroup>(ManualProviderGroupName);
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            var value = content;
            var qualifiedValue = new QualifiedValue(qualifier, value);

            localization.ValueManager.UpdateCreateValue(qualifiedValue);
            localization.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));

            localization.ValueManager.Persist();
            if (localization.ValueManager != localization.HistoryManager)
                localization.HistoryManager.Persist();

            return Json(new {ok = true});
        }

        [ValidateInput(false)]
        public JsonResult DeleteValue(String part, String locale, String key)
        {
            var localization = Session.Get<ProviderGroup>(ManualProviderGroupName);
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.DeleteValue(qualifier);

            localization.ValueManager.Persist();
            if (localization.ValueManager != localization.HistoryManager)
                localization.HistoryManager.Persist();

            return Json(new { ok = true });
        }

        private static void FixTree(IDictionary<Part, LocalizedPart> groupedParts, Part key)
        {
            var newKey = key.Parent;
            if (newKey == null || groupedParts.ContainsKey(newKey)) return;

            groupedParts.Add(newKey, new LocalizedPart(newKey, new List<QualifiedLocalization>(0)));
            FixTree(groupedParts, newKey);
        }
    }
}
