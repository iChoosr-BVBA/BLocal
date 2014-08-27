using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Controllers;

namespace BLocal.Web.Manager.Models
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