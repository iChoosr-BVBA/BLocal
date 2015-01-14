using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.ManualSynchronization;
using BLocal.Web.Manager.Providers.RemoteAccess;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class ManualSynchronizationController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public ManualSynchronizationController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }
        
        public ActionResult Index(String leftConfigName, String rightConfigName, Boolean hardReload = false)
        {
            var leftProviders = ProviderGroupFactory.CreateProviderGroup(leftConfigName);
            leftProviders.ValueManager.Reload();
            var leftValues = leftProviders.ValueManager.GetAllValuesQualified().ToArray();
            leftProviders.HistoryManager.AdjustHistory(leftValues, Session.Get<String>("author"));

            var rightProviders = ProviderGroupFactory.CreateProviderGroup(rightConfigName);
            rightProviders.ValueManager.Reload();
            var rightValues = rightProviders.ValueManager.GetAllValuesQualified().ToArray();
            rightProviders.HistoryManager.AdjustHistory(rightValues, Session.Get<String>("author"));

            var leftNotRight = leftValues.Where(lv => !rightValues.Select(rv => rv.Qualifier).Contains(lv.Qualifier))
                .Select(lv => new SynchronizationData.QualifiedHistoricalValue(lv, leftProviders.HistoryManager.GetHistory(lv.Qualifier))).ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.Select(lv => lv.Qualifier).Contains(rv.Qualifier))
                .Select(rv => new SynchronizationData.QualifiedHistoricalValue(rv, rightProviders.HistoryManager.GetHistory(rv.Qualifier))).ToArray();

            var valueDifferences = leftValues
                .Join(rightValues, v => v.Qualifier, v => v.Qualifier, (lv, rv) => new SynchronizationData.QualifiedHistoricalValuePair(
                    new SynchronizationData.QualifiedHistoricalValue(lv, leftProviders.HistoryManager.GetHistory(lv.Qualifier)),
                    new SynchronizationData.QualifiedHistoricalValue(rv, rightProviders.HistoryManager.GetHistory(rv.Qualifier))
                ))
                .Where(dv => !Equals(dv.Left.Value, dv.Right.Value))
                .ToArray();

            return View(new SynchronizationData(leftProviders, rightProviders, leftNotRight, rightNotLeft, valueDifferences));
        }

        [ValidateInput(false)]
        public JsonResult Remove(SynchronizationItem[] items, String leftProviderConfigName, String rightProviderConfigName)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();
            var sideProviders = new Dictionary<Side, ProviderGroup> {
                { Side.Left, ProviderGroupFactory.CreateProviderGroup(leftProviderConfigName) },
                { Side.Right, ProviderGroupFactory.CreateProviderGroup(rightProviderConfigName) }
            };

            if (items.Length > 5)
                foreach(var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.StartBatch();

            foreach (var item in items)
            {
                var localizationFrom = sideProviders[item.Side];
                var localizationTo = sideProviders[item.Side == Side.Left ? Side.Right : Side.Left];

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                EnsureHistoryLoaded(localizationFrom, qualifier);
                localizationTo.ValueManager.DeleteValue(qualifier);
                HistoryMerger.MergeHistory(qualifier, localizationFrom.HistoryManager, localizationTo.HistoryManager);
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }
            foreach (var localization in sourcedLocalizationSides.Except(changedLocalizationSides))
            {
                localization.HistoryManager.Persist();
            }

            if (items.Length > 5)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.EndBatch();

            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult Duplicate(SynchronizationItem[] items, String leftProviderConfigName, String rightProviderConfigName)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();
            var sideProviders = new Dictionary<Side, ProviderGroup> {
                { Side.Left, ProviderGroupFactory.CreateProviderGroup(leftProviderConfigName) },
                { Side.Right, ProviderGroupFactory.CreateProviderGroup(rightProviderConfigName) }
            };

            if (items.Length > 5)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.StartBatch();

            foreach (var item in items)
            {
                var localizationFrom = sideProviders[item.Side];
                var localizationTo = sideProviders[item.Side == Side.Left ? Side.Right : Side.Left];

                if (localizationFrom == null || localizationTo == null)
                    throw new Exception("Localization not loaded!");

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                EnsureHistoryLoaded(localizationFrom, qualifier);
                localizationTo.ValueManager.UpdateCreateValue(localizationFrom.ValueManager.GetQualifiedValue(qualifier));
                HistoryMerger.MergeHistory(qualifier, localizationFrom.HistoryManager, localizationTo.HistoryManager);
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if(localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }
            foreach (var localization in sourcedLocalizationSides.Except(changedLocalizationSides))
            {
                localization.HistoryManager.Persist();
            }

            if (items.Length > 5)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.EndBatch();

            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult Update(SynchronizationItem[] items, String leftProviderConfigName, String rightProviderConfigName)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();
            var sideProviders = new Dictionary<Side, ProviderGroup> {
                { Side.Left, ProviderGroupFactory.CreateProviderGroup(leftProviderConfigName) },
                { Side.Right, ProviderGroupFactory.CreateProviderGroup(rightProviderConfigName) }
            };

            if (items.Length > 5)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.StartBatch();

            foreach (var item in items)
            {
                var localizationFrom = sideProviders[item.Side];
                var localizationTo = sideProviders[item.Side == Side.Left ? Side.Right : Side.Left];

                if (localizationFrom == null || localizationTo == null)
                    throw new Exception("Localization not loaded!");

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                localizationTo.ValueManager.UpdateCreateValue(localizationFrom.ValueManager.GetQualifiedValue(qualifier));
                EnsureHistoryLoaded(localizationFrom, qualifier);
                HistoryMerger.MergeHistory(qualifier, localizationFrom.HistoryManager, localizationTo.HistoryManager);
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }
            foreach (var localization in sourcedLocalizationSides.Except(changedLocalizationSides))
            {
                localization.HistoryManager.Persist();
            }

            if (items.Length > 5)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.EndBatch();

            return Json(new { ok = true });
        }

        private void EnsureHistoryLoaded(ProviderGroup localizationFrom, Qualifier.Unique qualifier)
        {
            var historyFrom = localizationFrom.HistoryManager.GetHistory(qualifier);
            if (historyFrom == null || historyFrom.LatestEntry() == null)
                localizationFrom.HistoryManager.AdjustHistory(localizationFrom.ValueManager.GetAllValuesQualified(), Session.Get<String>("author"));
        }
    }
}