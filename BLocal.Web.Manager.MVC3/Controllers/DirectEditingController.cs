﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.DirectEditing;
using iChoosr.Sanitizer;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class DirectEditingController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }
        private ISanitizer _sanitizer;

        private ISanitizer Sanitizer
        {
            get
            {
                if (_sanitizer == null)
                    _sanitizer = HttpContext.Application["sanitizer"] as ISanitizer;

                return _sanitizer;
            }
        }

        public DirectEditingController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var localizations = localization.ValueManager.GetAllValuesQualified().ToList();
            var history = localization.HistoryManager.ProvideHistory().ToDictionary(h => h.Qualifier);

            var historyChecker = new HistoryChecker();
            historyChecker.ValidateHistory(localizations, history.Values, providerConfigName);

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

            var model = new IndexModel {Parts = groupedParts.Values, Provider = localization};
            return View(model);
        }

        [ValidateInput(false)]
        public JsonResult UpdateCreateValue(String part, String locale, String key, String content, String providerConfigName)
        {
            part = part.ToLower();
            locale = locale.ToLower();
            key = key.ToLower();

            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            var sanitizationResult =
                Sanitizer.Sanitize(content, SanitizationExclusionType.Cms, SanitizerOperatingMode.Input);

            if (sanitizationResult.ExternalizedErrors.Count > 0){
                return Json(new { ok = false,  sanitizationResult });
            } else { 
                var value = content;
                var qualifiedValue = new QualifiedValue(qualifier, value);

                localization.ValueManager.UpdateCreateValue(qualifiedValue);
                localization.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));

                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                    localization.HistoryManager.Persist();

                return Json(new {ok = true});
            }
        }

        [ValidateInput(false)]
        public JsonResult MoveAndUpdateValue(String oldPart, String oldLocale, String oldKey, String newPart, String newLocale, String newKey, String newContent, String providerConfigName)
        {
            var sanitizationResult =
                Sanitizer.Sanitize(newContent, SanitizationExclusionType.Cms, SanitizerOperatingMode.Input);

            if (sanitizationResult.ExternalizedErrors.Count > 0)
            {
                return Json(new {ok = false, sanitizationResult});
            }
            else
            {
                oldPart = oldPart.ToLower();
                oldLocale = oldLocale.ToLower();
                oldKey = oldKey.ToLower();

                newPart = newPart.ToLower();
                newLocale = newLocale.ToLower();
                newKey = newKey.ToLower();

                var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

                // delete old Qualifier
                var oldQualifier = new Qualifier.Unique(Part.Parse(oldPart), new Locale(oldLocale), oldKey);
                var oldQualifiedValue = new QualifiedValue(oldQualifier, null);

                localization.ValueManager.DeleteValue(oldQualifier);
                localization.HistoryManager.ProgressHistory(oldQualifiedValue, Session.Get<String>("author"));

                // insert new Qualifier
                var newQualifier = new Qualifier.Unique(Part.Parse(newPart), new Locale(newLocale), newKey);
                var newQualifiedValue = new QualifiedValue(newQualifier, newContent);

                localization.ValueManager.UpdateCreateValue(newQualifiedValue);
                localization.HistoryManager.ProgressHistory(newQualifiedValue, Session.Get<String>("author"));

                localization.ValueManager.Persist();
                if (localization.ValueManager != localization.HistoryManager)
                {
                    localization.HistoryManager.Persist();
                }

                return Json(new {ok = true});
            }
        }

        [ValidateInput(false)]
        public JsonResult DeleteValue(String part, String locale, String key, String providerConfigName)
        {
            
            var localization = ProviderGroupFactory.CreateProviderGroup(providerConfigName);

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.DeleteValue(qualifier);
            localization.HistoryManager.ProgressHistory(new QualifiedValue(qualifier, null), Session.Get<String>("author"));

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
