using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ProgressHistoryResponse
    {
        public IEnumerable<QualifiedHistory> History { get; set; } 
    }
}