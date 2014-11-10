using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.ImportExport
{
    public class ImportReportData
    {
        public readonly String ProviderGroupName;
        public readonly String UploadedFileName;
        public readonly Locale AffectedLocale;
        public readonly List<QualifiedValue> Inserts;
        public readonly List<Tuple<QualifiedValue, ImportExportRecord>> Updates;
        public readonly List<QualifiedValue> Deletes;

        public ImportReportData(string providerGroupName, string uploadedFileName, Locale affectedLocale, List<QualifiedValue> inserts, List<Tuple<QualifiedValue, ImportExportRecord>> updates, List<QualifiedValue> deletes)
        {
            ProviderGroupName = providerGroupName;
            UploadedFileName = uploadedFileName;
            AffectedLocale = affectedLocale;
            Inserts = inserts;
            Updates = updates;
            Deletes = deletes;
        }
    }
}