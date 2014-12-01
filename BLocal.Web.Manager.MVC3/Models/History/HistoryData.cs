using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Models.History
{
    public class HistoryData
    {
        public String ProviderName { get; set; }
        public List<QualifiedHistory> History { get; set; }
    }
}