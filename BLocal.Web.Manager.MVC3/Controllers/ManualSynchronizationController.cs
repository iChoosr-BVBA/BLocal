﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
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

        private static QualifiedHistory MergeHistory(Qualifier.Unique qualifier, ProviderGroup group1, ProviderGroup group2)
        {
            var group1History = group1.HistoryManager.GetHistory(qualifier);
            var group2History = group2.HistoryManager.GetHistory(qualifier);
            return HistoryMerger.MergeHistoryEntries(group1History, group2History);
        }

        public ActionResult Index(String leftConfigName, String rightConfigName, Boolean hardReload = false)
        {
            var leftProviders = ProviderGroupFactory.CreateProviderGroup(leftConfigName);
            leftProviders.ValueManager.Reload();
            var leftValues = leftProviders.ValueManager.GetAllValuesQualified().ToArray();
            
            var rightProviders = ProviderGroupFactory.CreateProviderGroup(rightConfigName);
            rightProviders.ValueManager.Reload();
            var rightValues = rightProviders.ValueManager.GetAllValuesQualified().ToArray();

            var historyChecker = new HistoryChecker();
            historyChecker.ValidateHistory(leftValues, leftProviders.HistoryManager.ProvideHistory(), leftConfigName);
            historyChecker.ValidateHistory(rightValues, rightProviders.HistoryManager.ProvideHistory(), rightConfigName);

            var leftNotRight = leftValues.Where(lv => !rightValues.Select(rv => rv.Qualifier).Contains(lv.Qualifier))
                .Select(lv => new SynchronizationData.QualifiedHistoricalValue(lv, MergeHistory(lv.Qualifier, leftProviders, rightProviders)))
                .ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.Select(lv => lv.Qualifier).Contains(rv.Qualifier))
                .Select(rv => new SynchronizationData.QualifiedHistoricalValue(rv, MergeHistory(rv.Qualifier, leftProviders, rightProviders)))
                .ToArray();

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

            if (items.Length >= 3)
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

            if (items.Length >= 5)
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

            if (items.Length >= 3)
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

            if (items.Length >= 3)
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

            if (items.Length >= 3)
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
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }
            foreach (var localization in sourcedLocalizationSides.Except(changedLocalizationSides))
            {
                localization.HistoryManager.Persist();
            }

            if (items.Length >= 3)
                foreach (var manager in sideProviders.Select(p => p.Value.ValueManager).OfType<RemoteAccessManager>())
                    manager.EndBatch();

            return Json(new { ok = true });
        }

        private void EnsureHistoryLoaded(ProviderGroup localizationFrom, Qualifier.Unique qualifier)
        {
            var historyFrom = localizationFrom.HistoryManager.GetHistory(qualifier);
            if (historyFrom == null || historyFrom.LatestEntry() == null)
                throw new HistoryChecker.HistoryConflictException(String.Format("Cannot find history for {0} on {1}", qualifier, localizationFrom.Name));
        }
    }
}