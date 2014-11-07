using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class OverrideHistoryResponse
    {
        public IEnumerable<QualifiedHistory> History { get; set; }
    }
}