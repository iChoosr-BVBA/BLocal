using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Context;
using BLocal.Web.Manager.Extensions;
using BLocal.Web.Manager.Models.ImportExport;
using BLocal.Web.Manager.Models.TranslationVerification;
using CsvHelper;

namespace BLocal.Web.Manager.Controllers
{
    [Authenticate]
    public class ImportExportController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public ImportExportController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ActionResult Index(String providerConfigName)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray();
            var allParts = allValues.Select(value => value.Qualifier.Part).Distinct().OrderBy(p => p.ToString()).ToArray();
            var allLocales = allValues.Select(value => value.Qualifier.Locale).Distinct().OrderBy(l => l.ToString()).ToArray();

            var partNodes = new Dictionary<Part, PartNode>();
            foreach (var part in allParts)
            {
                var isRoot = part.Parent == null;
                var node = new PartNode(part, isRoot);

                partNodes.Add(part, node);
                if (!isRoot)
                {
                    var fixNode = node;
                    while (fixNode.Part.Parent != null)
                    {
                        var parentNode = new PartNode(fixNode.Part.Parent, fixNode.Part.Parent.Parent == null);
                        if (partNodes.ContainsKey(parentNode.Part))
                        {
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

            return View(new ImportExportData(partNodes.Values, allLocales, providerGroup));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Export(String providerConfigName, String[] parts, String format, String locale)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray();
            var selectedLocale = new Locale(locale);
            var groupedTranslations = allValues
                .Where(v => parts.Contains(v.Qualifier.Part.ToString()))
                .ToLookup(v => new Qualifier(v.Qualifier.Part, TranslationVerificationData.NoLocale, v.Qualifier.Key));

            var records = groupedTranslations.Select(group =>
                group.SingleOrDefault(v => Equals(v.Qualifier.Locale.ToString(), locale))
                    ?? new QualifiedValue(
                        new Qualifier.Unique(group.Key.Part, selectedLocale, group.Key.Key),
                        String.Empty
                    )
            ).Select(v => new ImportExportRecord(v.Qualifier.Part.ToString(), v.Qualifier.Key, v.Value))
            .ToArray();

            var stream = new MemoryStream();
            try
            {
                switch (format.ToLowerInvariant())
                {
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
            catch
            {
                stream.Dispose();
                return null;
            }
            throw new Exception("Unknown data format: " + format);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Import(String providerConfigName, String locale, HttpPostedFileBase postedFile)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(providerConfigName);
            var allValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray();
            var selectedLocale = new Locale(locale);
            var valuesByPartKey = allValues
                .Where(value => value.Qualifier.Locale.Equals(selectedLocale))
                .ToDictionary(v => new Qualifier.Unique(v.Qualifier.Part, selectedLocale, v.Qualifier.Key));

            var extention = postedFile.FileName.Split('.').Last();
            var records = new List<ImportExportRecord>();
            switch (extention)
            {
                case "csv":
                    using (var reader = new CsvReader(new StreamReader(postedFile.InputStream)))
                    {
                        reader.Configuration.Delimiter = ";";
                        records.AddRange(reader.GetRecords<ImportExportRecord>());
                    }
                    break;
                case "xml":
                    // ReSharper disable PossibleNullReferenceException
                    var document = XDocument.Load(postedFile.InputStream);
                    records.AddRange(document.Root.Descendants("Localization").Select(
                        xel => new ImportExportRecord(xel.Element("Part").Value, xel.Element("Key").Value, xel.Element("Value").Value) { DeleteOnImport = xel.Element("DeleteOnImport").Value.Trim().ToLowerInvariant() != "false" }
                    ));
                    // ReSharper restore PossibleNullReferenceException
                    break;
            }

            var inserts = new List<QualifiedValue>();
            var updates = new List<Tuple<QualifiedValue, ImportExportRecord>>();
            var deletes = new List<QualifiedValue>();

            foreach (var record in records)
            {
                QualifiedValue correspondingValue;
                var recordQualfier = new Qualifier.Unique(Part.Parse(record.Part), selectedLocale, record.Key);

                if (valuesByPartKey.TryGetValue(recordQualfier, out correspondingValue))
                {
                    if (record.DeleteOnImport)
                        deletes.Add(correspondingValue);
                    else if (!record.Value.Equals(correspondingValue.Value))
                        updates.Add(Tuple.Create(correspondingValue, record));
                }
                else if (!record.DeleteOnImport)
                    inserts.Add(new QualifiedValue(recordQualfier, record.Value));
            }

            providerGroup.ValueManager.Persist();
            return View(new ImportReportData(providerConfigName, postedFile.FileName, selectedLocale, inserts, updates, deletes));
        }

        [ValidateInput(false)]
        public JsonResult FinalizeUpdate(ImportConfiguration configuration)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var update in configuration.Data)
            {
                var qualifiedValue = new QualifiedValue(new Qualifier.Unique(Part.Parse(update.Part), selectedLocale, update.Key), update.Value);
                providerGroup.ValueManager.UpdateCreateValue(qualifiedValue);
                providerGroup.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));
            }
            providerGroup.ValueManager.Persist();
            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult FinalizeInsert(ImportConfiguration configuration)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var insert in configuration.Data)
            {
                var qualifiedValue = new QualifiedValue(new Qualifier.Unique(Part.Parse(insert.Part), selectedLocale, insert.Key), insert.Value);
                providerGroup.ValueManager.CreateValue(qualifiedValue.Qualifier, qualifiedValue.Value);
                providerGroup.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));
            }
            providerGroup.ValueManager.Persist();
            return Json(new { ok = true });
        }

        [ValidateInput(false)]
        public JsonResult FinalizeDelete(ImportConfiguration configuration)
        {
            var providerGroup = ProviderGroupFactory.CreateProviderGroup(configuration.ProviderConfigName);
            var selectedLocale = new Locale(configuration.Locale);
            foreach (var delete in configuration.Data)
            {
                var qualifiedValue = new QualifiedValue(new Qualifier.Unique(Part.Parse(delete.Part), selectedLocale, delete.Key), null);
                providerGroup.ValueManager.DeleteValue(qualifiedValue.Qualifier);
                providerGroup.HistoryManager.ProgressHistory(qualifiedValue, Session.Get<String>("author"));
            }
            providerGroup.ValueManager.Persist();
            return Json(new { ok = true });
        }
    }
}
