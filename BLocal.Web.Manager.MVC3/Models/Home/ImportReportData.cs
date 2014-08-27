using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Controllers;

namespace BLocal.Web.Manager.Models
{
    public class ImportReportData
    {
        public readonly String ProviderPairName;
        public readonly String UploadedFileName;
        public readonly Locale AffectedLocale;
        public readonly List<QualifiedValue> Inserts;                                 
        public readonly List<Tuple<QualifiedValue, ImportExportRecord>> Updates;      
        public readonly List<QualifiedValue> Deletes;

        public ImportReportData(string providerPairName, string uploadedFileName, Locale affectedLocale, List<QualifiedValue> inserts, List<Tuple<QualifiedValue, ImportExportRecord>> updates, List<QualifiedValue> deletes)
        {
            ProviderPairName = providerPairName;
            UploadedFileName = uploadedFileName;
            AffectedLocale = affectedLocale;
            Inserts = inserts;
            Updates = updates;
            Deletes = deletes;
        }
    }
}