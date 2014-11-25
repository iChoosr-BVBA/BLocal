using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.ManualSynchronization;

namespace BLocal.Web.Manager.Controllers
{
    public class ManualSynchronizationController : Controller
    {
        private const String SynchronizationProviderGroupNameBase = "synchronization-{0}";

        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public ManualSynchronizationController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }
        
        public ActionResult Index(String leftConfigName, String rightConfigName, Boolean hardReload = false)
        {
            var leftProviders = Session.GetOrSetDefault(String.Format(SynchronizationProviderGroupNameBase, "Left"), ProviderGroupFactory.CreateProviderGroup(leftConfigName));
            var rightProviders = Session.GetOrSetDefault(String.Format(SynchronizationProviderGroupNameBase, "Right"), ProviderGroupFactory.CreateProviderGroup(rightConfigName));

            leftProviders.ValueManager.Reload();
            rightProviders.ValueManager.Reload();

            var leftValues = leftProviders.ValueManager.GetAllValuesQualified().ToArray();
            var rightValues = rightProviders.ValueManager.GetAllValuesQualified().ToArray();

            leftProviders.HistoryManager.AdjustHistory(leftValues, Session.Get<String>("author"));
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

            return View(new SynchronizationData(leftProviders.Name, rightProviders.Name, leftNotRight, rightNotLeft, valueDifferences));
        }

        [ValidateInput(false)]
        public JsonResult Remove(SynchronizationItem[] items)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();

            foreach (var item in items)
            {
                var localizationFrom = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, item.Side));
                var localizationTo = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, item.Side));
                if (localizationTo == null)
                    throw new Exception("Localization not loaded!");

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                localizationTo.ValueManager.DeleteValue(qualifier);

                // make sure that the "from" is also on the latest history
                localizationFrom.HistoryManager.ProgressHistory(new QualifiedValue(qualifier, null), Session.Get<String>("author"));
                localizationTo.HistoryManager.OverrideHistory(localizationFrom.HistoryManager.GetHistory(qualifier));
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }

            // persist any changed history on the sources that has already been persisted
            foreach (var localization in sourcedLocalizationSides)
            {
                if (!changedLocalizationSides.Contains(localization))
                    localization.HistoryManager.Persist();
            }

            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult Duplicate(SynchronizationItem[] items)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();

            foreach (var item in items)
            {
                var localizationFrom = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, item.Side));
                var localizationTo = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, (item.Side == "Right" ? "Left" : "Right")));

                if (localizationFrom == null || localizationTo == null)
                    throw new Exception("Localization not loaded!");

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                localizationTo.ValueManager.UpdateCreateValue(localizationFrom.ValueManager.GetQualifiedValue(qualifier));

                // make sure that the "from" is also on the latest history
                localizationFrom.HistoryManager.ProgressHistory(localizationFrom.ValueManager.GetQualifiedValue(qualifier), Session.Get<String>("author"));
                localizationTo.HistoryManager.OverrideHistory(localizationFrom.HistoryManager.GetHistory(qualifier));
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if(localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }

            // persist any changed history on the sources that has already been persisted
            foreach (var localization in sourcedLocalizationSides)
            {
                if(!changedLocalizationSides.Contains(localization))
                    localization.HistoryManager.Persist();
            }

            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult Update(SynchronizationItem[] items)
        {
            var changedLocalizationSides = new HashSet<ProviderGroup>();
            var sourcedLocalizationSides = new HashSet<ProviderGroup>();

            foreach (var item in items)
            {
                var localizationFrom = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, item.Side));
                var localizationTo = Session.Get<ProviderGroup>(String.Format(SynchronizationProviderGroupNameBase, (item.Side == "Right" ? "Left" : "Right")));

                if (localizationFrom == null || localizationTo == null)
                    throw new Exception("Localization not loaded!");

                if (!changedLocalizationSides.Contains(localizationTo))
                    changedLocalizationSides.Add(localizationTo);
                if (!sourcedLocalizationSides.Contains(localizationFrom))
                    sourcedLocalizationSides.Add(localizationFrom);

                var qualifier = new Qualifier.Unique(Part.Parse(item.Part), new Locale(item.Locale), item.Key);
                localizationTo.ValueManager.UpdateCreateValue(localizationFrom.ValueManager.GetQualifiedValue(qualifier));

                // make sure that the "from" is also on the latest history
                localizationFrom.HistoryManager.ProgressHistory(localizationFrom.ValueManager.GetQualifiedValue(qualifier), Session.Get<String>("author"));
                localizationTo.HistoryManager.OverrideHistory(localizationFrom.HistoryManager.GetHistory(qualifier));
            }

            // persist any changes
            foreach (var localization in changedLocalizationSides)
            {
                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();
            }

            // persist any changed history on the sources that has already been persisted
            foreach (var localization in sourcedLocalizationSides)
            {
                if (!changedLocalizationSides.Contains(localization))
                    localization.HistoryManager.Persist();
            }

            return Json(new { ok = true });
        }
    }
}