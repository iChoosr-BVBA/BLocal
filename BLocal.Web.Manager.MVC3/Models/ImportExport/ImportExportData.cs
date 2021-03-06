﻿using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.ImportExport
{
    public class ImportExportData
    {
        public readonly IEnumerable<PartNode> PartNodes;
        public readonly Locale[] Locales;
        public readonly ProviderGroup Provider;

        public ImportExportData(IEnumerable<PartNode> partNodes, Locale[] locales, ProviderGroup provider)
        {
            PartNodes = partNodes;
            Locales = locales;
            Provider = provider;
        }
    }
}