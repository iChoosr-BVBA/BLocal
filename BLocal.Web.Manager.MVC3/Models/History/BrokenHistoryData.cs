using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.History
{
    public class BrokenHistoryData
    {
        public List<HistoryChecker.QualifiedConflict> ConflictingValues { get; set; }
        public ProviderGroup Provider { get; set; }
    }
}