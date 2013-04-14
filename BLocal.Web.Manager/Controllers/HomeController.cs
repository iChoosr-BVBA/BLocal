using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Configuration;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Models;
using CsvHelper;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Overview()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Authenticate(String password)
        {
            if(password == ConfigurationManager.AppSettings["password"])
                Session["auth"] = DateTime.Now;
            return RedirectToAction("Overview");
        }

        [HttpPost]
        public ActionResult LoadLocalization(String providerConfigName)
        {
            Session["manualProviderPair"] = null;
            Session["manualProviderPair"] = FetchProviderPair(providerConfigName);
            return RedirectToAction("EditLocalization");
        }

        [HttpPost]
        public ActionResult Synchronize(String leftConfigName, String rightConfigName)
        {
            Session["synchronizationLeftProviderPair"] = null;
            Session["synchronizationRightProviderPair"] = null;
            Session["synchronizationLeftProviderPair"] = FetchProviderPair(leftConfigName);
            Session["synchronizationRightProviderPair"] = FetchProviderPair(rightConfigName);
            return RedirectToAction("ShowSynchronization");
        }

        [HttpPost]
        public ActionResult LoadTranslations(String providerConfigName)
        {
            Session["translationProviderPair"] = null;
            Session["translationProviderPair"] = FetchProviderPair(providerConfigName);
            return RedirectToAction("VerifyTranslation");
        }

        public ActionResult EditLocalization()
        {
            var localization = Session["manualProviderPair"] as ProviderPair;
            if (localization == null)
                return RedirectToAction("Overview");

            var logs = localization.Logger.GetLatestLogsBetween(DateTime.Now.Subtract(TimeSpan.FromDays(50)), DateTime.Now);
            var localizations = localization.ValueManager.GetAllValuesQualified().ToList();

            var groupedParts = localizations
                .GroupJoin(logs, loc => loc.Qualifier, log => log.Key, (loc, loclogs) =>
                    new QualifiedLocalization(loc.Qualifier, loc.Value, loclogs.Select(l => l.Value.Date).FirstOrDefault())
                )
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

            return View(groupedParts.Values);
        }

        public ActionResult ShowSynchronization(bool hardReload = false)
        {
            var leftPair = (ProviderPair) Session["synchronizationLeftProviderPair"];
            var rightPair = (ProviderPair) Session["synchronizationRightProviderPair"];

            if (hardReload) {
                leftPair.ValueManager.Reload();
                rightPair.ValueManager.Reload();
            }

            var leftValues = leftPair.ValueManager.GetAllValuesQualified().ToArray();
            var rightValues = rightPair.ValueManager.GetAllValuesQualified().ToArray();

            var leftNotRight = leftValues.Where(lv => !rightValues.Select(rv => rv.Qualifier).Contains(lv.Qualifier)).ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.Select(lv => lv.Qualifier).Contains(rv.Qualifier)).ToArray();

            var valueDifferences = leftValues
                .Join(rightValues, v => v.Qualifier, v => v.Qualifier, (lv, rv) => new SynchronizationData.DoubleQualifiedValue(lv, rv))
                .Where(dv => !Equals(dv.Left.Value, dv.Right.Value))
                .ToArray();

            return View(new SynchronizationData(leftPair.Name, rightPair.Name, leftNotRight, rightNotLeft, valueDifferences));
        }

        public ActionResult VerifyTranslation()
        {
            var localization = Session["translationProviderPair"] as ProviderPair;
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

        public ActionResult ReloadLocalization()
        {
            var localization = Session["manualProviderPair"] as ProviderPair;
            if (localization == null)
                return RedirectToAction("Index");

            localization.ValueManager.Reload();
            return RedirectToAction("EditLocalization");
        }

        public ActionResult ShowImportExport(String providerConfigName)
        {
            var providerPair = FetchProviderPair(providerConfigName);
            var allValues = providerPair.ValueManager.GetAllValuesQualified().ToArray();
            var allParts = allValues.Select(value => value.Qualifier.Part).Distinct().OrderBy(p => p.ToString()).ToArray();
            var allLocales = allValues.Select(value => value.Qualifier.Locale).Distinct().OrderBy(l => l.ToString()).ToArray();

            var partNodes = new Dictionary<Part, PartNode>();
            foreach (var part in allParts) {
                var isRoot = part.Parent == null;
                var node = new PartNode(part, isRoot);

                partNodes.Add(part, node);
                if (!isRoot) {
                    var fixNode = node;
                    while (fixNode.Part.Parent != null) {
                        var parentNode = new PartNode(fixNode.Part.Parent, fixNode.Part.Parent.Parent == null);
                        if (partNodes.ContainsKey(parentNode.Part)) {
                            partNodes[parentNode.Part].SubParts.Add(fixNode);
                            break;
                        }

                        partNodes.Add(parentNode.Part, parentNode);
                        parentNode.SubParts.Add(fixNode);
                        fixNode = parentNode;
                    }
                }
            }
            foreach (var part in allParts.Where(part => part.Parent != null))
                partNodes.Remove(part);

            return View(new ImportExportData(partNodes.Values, allLocales, providerConfigName));
        }

        [HttpPost]
        public ActionResult Export(String providerConfigName, String[] parts, String format, String locale)
        {
            var providerPair = FetchProviderPair(providerConfigName);
            var allValues = providerPair.ValueManager.GetAllValuesQualified().ToArray();
            var selectedLocale = new Locale(locale);
            var groupedTranslations = allValues
                .Where(v => parts.Contains(v.Qualifier.Part.ToString()))
                .ToLookup(v => new Qualifier(v.Qualifier.Part, TranslationVerificationData.NoLocale, v.Qualifier.Key));

            var records = groupedTranslations.Select(group =>
                group.SingleOrDefault(v => Equals(v.Qualifier.Locale.ToString(), locale))
                    ?? new QualifiedValue(
                        new Qualifier.Unique(group.Key.Part, selectedLocale, group.Key.Key),
                        new Value(ContentType.Unspecified, String.Empty)
                    )
            ).Select(v => new ImportExportRecord(v.Qualifier.Part.ToString(), v.Qualifier.Key, v.Value.Content))
            .ToArray();

            var stream = new MemoryStream();
            try {
                switch (format.ToLowerInvariant()) {
                    case "csv":
                        var streamWriter = new StreamWriter(stream);
                        var csvWriter = new CsvWriter(streamWriter);
                        csvWriter.Configuration.Delimiter = ";";

                        csvWriter.WriteRecords(records);
                        streamWriter.Flush();
                        stream.Seek(0, SeekOrigin.Begin);

                        return File(stream, "text/csv", providerConfigName + " - " + selectedLocale + " - " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".csv");
                    case "xml":
                        var xmlRecords = new XElement("Localizations",
                            records.Select(r => new XElement("Localization",
                                new XElement("Part", r.Part),
                                new XElement("Key", r.Key),
                                new XElement("Value", r.Value),
                                new XElement("DeleteOnImport", r.DeleteOnImport)
                            ))
                        );

                        xmlRecords.Save(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        return File(stream, "text/xml", providerConfigName + " - " + selectedLocale + " - " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".xml");
                }
            }
            catch {
                stream.Dispose();
                return null;
            }
            throw new Exception("Unknown data format: " + format);
        }

        [HttpPost]
        public ActionResult Import(String providerConfigName, String locale, HttpPostedFileBase postedFile)
        {
            var providerPair = FetchProviderPair(providerConfigName);
            var allValues = providerPair.ValueManager.GetAllValuesQualified().ToArray();
            var selectedLocale = new Locale(locale);
            var valuesByPartKey = allValues
                .Where(value => value.Qualifier.Locale.Equals(selectedLocale))
                .ToDictionary(v => new Qualifier.Unique(v.Qualifier.Part, selectedLocale, v.Qualifier.Key));
            
            var extention = postedFile.FileName.Split('.').Last();
            var records = new List<ImportExportRecord>();
            switch (extention) {
                case "csv":
                    using (var reader = new CsvReader(new StreamReader(postedFile.InputStream))) {
                        reader.Configuration.Delimiter = ";";
                        records.AddRange(reader.GetRecords<ImportExportRecord>());
                    }
                    break;
                case "xml":
                    // ReSharper disable PossibleNullReferenceException
                    var document = XDocument.Load(postedFile.InputStream);
                    records.AddRange(document.Root.Descendants("Localization").Select(
                        xel => new ImportExportRecord(xel.Element("Part").Value, xel.Element("Key").Value, xel.Element("Value").Value)
                            { DeleteOnImport = xel.Element("DeleteOnImport").Value.Trim().ToLowerInvariant() != "false" }
                    ));
                    // ReSharper restore PossibleNullReferenceException
                    break;
            }

            var inserts = new List<QualifiedValue>();
            var updates = new List<Tuple<QualifiedValue, ImportExportRecord>>();
            var deletes = new List<QualifiedValue>();

            foreach (var record in records) {
                QualifiedValue correspondingValue;
                var recordQualfier = new Qualifier.Unique(Part.Parse(record.Part), selectedLocale, record.Key);

                if (valuesByPartKey.TryGetValue(recordQualfier, out correspondingValue)) {
                    if(record.DeleteOnImport)
                        deletes.Add(correspondingValue);
                    else if(!record.Value.Equals(correspondingValue.Value.Content))
                        updates.Add(Tuple.Create(correspondingValue, record));
                }
                else if(!record.DeleteOnImport)
                    inserts.Add(new QualifiedValue(recordQualfier, new Value(ContentType.Unspecified, record.Value)));
            }

            return View(new ImportReportData(providerConfigName, postedFile.FileName, selectedLocale, inserts, updates, deletes));
        }

        public JsonResult Create(String part, String locale, String key, String contentType, String content)
        {
            var localization = Session["manualProviderPair"] as ProviderPair;
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            var value = new Value(ContentType.Spoof(contentType), content);
            var qualifiedValue = new QualifiedValue(qualifier, value);
            localization.ValueManager.UpdateCreateValue(qualifiedValue);

            return Json(new {ok = true});
        }

        public JsonResult Remove(String part, String locale, String key)
        {
            var localization = Session["manualProviderPair"] as ProviderPair;
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.DeleteValue(qualifier);

            return Json(new { ok = true });
        }

        public JsonResult SyncRemove(String side, String part, String locale, String key)
        {
            var localization = Session["synchronization" + side + "ProviderPair"] as ProviderPair;
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.DeleteValue(qualifier);

            return Json(new { ok = true });
        }

        public JsonResult SyncDuplicate(String side, String part, String locale, String key)
        {
            var localizationFrom = Session["synchronization" + side + "ProviderPair"] as ProviderPair;
            var localizationTo = Session["synchronization" + (side == "Right" ? "Left" : "Right") + "ProviderPair"] as ProviderPair;

            if (localizationFrom == null || localizationTo == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localizationTo.ValueManager.UpdateCreateValue(localizationFrom.ValueManager.GetQualifiedValue(qualifier));

            return Json(new { ok = true });
        }

        public JsonResult TransUpdate(String part, String locale, String key, String value)
        {
            var localization = Session["translationProviderPair"] as ProviderPair;
            if (localization == null)
                throw new Exception("Localization not loaded!");

            var qualifier = new Qualifier.Unique(Part.Parse(part), new Locale(locale), key);
            localization.ValueManager.UpdateCreateValue(new QualifiedValue(qualifier, new Value(ContentType.Unspecified, value)));

            return Json(new { ok = true });
        }

        public JsonResult TransDelete(String part, String key)
        {
            var localization = Session["translationProviderPair"] as ProviderPair;
            if (localization == null)
                throw new Exception("Localization not loaded!");

            localization.ValueManager.DeleteLocalizationsFor(Part.Parse(part), key);

            return Json(new { ok = true });
        }

        public JsonResult ImportFinalizeUpdate(ImportConfiguration configuration)
        {
            var providerPair = FetchProviderPair(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var update in configuration.Data) {
                providerPair.ValueManager.UpdateCreateValue(new QualifiedValue(
                    new Qualifier.Unique(Part.Parse(update.Part), selectedLocale, update.Key),
                    new Value(ContentType.Unspecified, update.Value)
                ));
            }
            return Json(new { ok = true });
        }
        public JsonResult ImportFinalizeInsert(ImportConfiguration configuration)
        {
            var providerPair = FetchProviderPair(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var insert in configuration.Data) {
                providerPair.ValueManager.CreateValue(
                    new Qualifier.Unique(Part.Parse(insert.Part), selectedLocale, insert.Key),
                    insert.Value
                );
            }
            return Json(new { ok = true });
        }
        public JsonResult ImportFinalizeDelete(ImportConfiguration configuration)
        {
            var providerPair = FetchProviderPair(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var delete in configuration.Data) {
                providerPair.ValueManager.DeleteValue(new Qualifier.Unique(Part.Parse(delete.Part), selectedLocale, delete.Key));
            }
            return Json(new { ok = true });
        }

        private static ProviderPair FetchProviderPair(String providerConfigName)
        {
            var providerConfig = ProviderConfig.ProviderPairs.Single(vp => vp.Name == providerConfigName);

            var valueProviderType = Type.GetType(providerConfig.ValueProvider.Type);
            if(valueProviderType == null)
                throw new Exception("Cannot find type \"" + providerConfig.ValueProvider.Type + "\"");
            if(!typeof(ILocalizedValueManager).IsAssignableFrom(valueProviderType))
                throw new Exception("Type \"" + valueProviderType + "\" does not implement " + typeof(ILocalizedValueManager).Name + "!");

            var valueProvider = ConstructProvider(valueProviderType, providerConfig.ValueProvider.ConstructorArguments.Cast<ConstructorArgumentElement>().ToArray());
            if (valueProvider == null)
                throw new Exception("Could not initialize value provider, incorrect constructor arguments!");

            if (providerConfig.LogProvider.IsValueProvider)
                return new ProviderPair(providerConfigName, valueProvider as ILocalizedValueManager, valueProvider as ILocalizationLogger);

            var logProviderType = Type.GetType(providerConfig.LogProvider.Type);
            if (logProviderType == null)
                throw new Exception("Cannot find type \"" + providerConfig.LogProvider.Type + "\"");
            if (!typeof(ILocalizationLogger).IsAssignableFrom(logProviderType))
                throw new Exception("Type \"" + logProviderType + "\" does not implement " + typeof(ILocalizationLogger).Name + "!");

            var logProvider = ConstructProvider(logProviderType, providerConfig.LogProvider.ConstructorArguments.Cast<ConstructorArgumentElement>().ToArray());
            if (logProvider == null)
                throw new Exception("Could not initialize log provider, incorrect constructor arguments!");

            return new ProviderPair(providerConfigName, valueProvider as ILocalizedValueManager, logProvider as ILocalizationLogger);
        }

        private static Object ConstructProvider(Type providerType, ICollection<ConstructorArgumentElement> arguments)
        {
            foreach (var constructor in providerType.GetConstructors()) {
                var parameters = constructor.GetParameters().ToArray();

                // not all arguments can be specified
                if (parameters.Select(param => param.Name).Intersect(arguments.Select(a => a.Name)).Count() != arguments.Count)
                    continue;

                // not enough arguments to call constructor
                if (parameters.Any(param => !(param.IsOptional || param.HasDefaultValue || arguments.Any(arg => arg.Name == param.Name))))
                    continue;

                // get correct arguments
                var invocationArguments = parameters
                    .OrderBy(param => param.Position)
                    .Select(p => new { Parameter = p, Argument = arguments.SingleOrDefault(a => a.Name == p.Name) })
                    .Select(pair => pair.Argument == null ? pair.Parameter.DefaultValue : pair.Argument.Value)
                    .ToArray();

                // invoke constructor
                return constructor.Invoke(invocationArguments);
            }
            return null;
        }

        private static void FixTree(IDictionary<Part, LocalizedPart> groupedParts, Part key)
        {
            var newKey = key.Parent;
            if (newKey == null || groupedParts.ContainsKey(newKey)) return;

            groupedParts.Add(newKey, new LocalizedPart(newKey, new List<QualifiedLocalization>(0)));
            FixTree(groupedParts, newKey);
        }

        public class PartNode
        {
            public readonly Part Part;
            public readonly List<PartNode> SubParts;
            public readonly bool IsRoot;

            public PartNode(Part part, bool isRoot)
            {
                Part = part;
                SubParts = new List<PartNode>();
                IsRoot = isRoot;
            }

            protected bool Equals(PartNode other)
            {
                return Equals(Part, other.Part);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return Equals(obj as PartNode);
            }

            public override int GetHashCode()
            {
                return (Part != null ? Part.GetHashCode() : 0);
            }
        }

        public class LocalizedPart
        {
            public List<LocalizedPart> Subparts { get; set; }
            public List<QualifiedLocalization> Localizations { get; set; }
            public Part Part { get; set; }

            public LocalizedPart(Part part, List<QualifiedLocalization> localizations)
            {
                Part = part;
                Localizations = localizations;
                Subparts = new List<LocalizedPart>();
            }
        }

        public class ProviderPair
        {
            public readonly String Name;
            public readonly ILocalizedValueManager ValueManager;
            public readonly ILocalizationLogger Logger;

            public ProviderPair(String name, ILocalizedValueManager valueManager, ILocalizationLogger logger)
            {
                Name = name;
                ValueManager = valueManager;
                Logger = logger;
            }
        }
        public class ImportExportRecord
        {
            public String Part { get; set; }
            public String Key { get; set; }
            public String Value { get; set; }
            public bool DeleteOnImport { get; set; }

            public ImportExportRecord()
            {
            }

            public ImportExportRecord(string part, string key, string value)
            {
                Part = part;
                Key = key;
                Value = value;
                DeleteOnImport = false;
            }
        }
        public class ImportData
        {
            public String Part { get; set; }
            public String Key { get; set; }
            public String Value { get; set; }
        }
        public class ImportConfiguration
        {
            public String ProviderConfigName { get; set; }
            public String Locale { get; set; }
            public ImportData[] Data { get; set; }
        }
    }
}
