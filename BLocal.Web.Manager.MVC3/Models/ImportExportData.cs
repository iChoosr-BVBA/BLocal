using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.MVC3.Controllers;

namespace BLocal.Web.Manager.Models
{
    public class ImportExportData
    {
        public readonly IEnumerable<HomeController.PartNode> PartNodes;
        public readonly Locale[] Locales;
        public readonly String ProviderConfigName;

        public ImportExportData(IEnumerable<HomeController.PartNode> partNodes, Locale[] locales, String providerConfigName)
        {
            PartNodes = partNodes;
            Locales = locales;
            ProviderConfigName = providerConfigName;
        }
    }
}