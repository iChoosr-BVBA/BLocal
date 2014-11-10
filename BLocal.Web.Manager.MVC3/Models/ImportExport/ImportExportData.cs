using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.ImportExport
{
    public class ImportExportData
    {
        public readonly IEnumerable<PartNode> PartNodes;
        public readonly Locale[] Locales;
        public readonly String ProviderConfigName;

        public ImportExportData(IEnumerable<PartNode> partNodes, Locale[] locales, String providerConfigName)
        {
            PartNodes = partNodes;
            Locales = locales;
            ProviderConfigName = providerConfigName;
        }
    }
}