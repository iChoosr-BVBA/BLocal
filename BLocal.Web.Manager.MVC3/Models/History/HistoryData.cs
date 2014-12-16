using System;
using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.History
{
    public class HistoryData
    {
        public List<QualifiedHistory> History { get; set; }
        public ProviderGroup Provider { get; set; }
    }
}