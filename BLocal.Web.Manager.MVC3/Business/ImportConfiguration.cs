using System;
using BLocal.Web.Manager.Controllers;

namespace BLocal.Web.Manager.Business
{
    public class ImportConfiguration
    {
        public String ProviderConfigName { get; set; }
        public String Locale { get; set; }
        public ImportData[] Data { get; set; }
    }
}