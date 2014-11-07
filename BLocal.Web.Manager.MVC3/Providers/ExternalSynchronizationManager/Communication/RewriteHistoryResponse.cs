using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class RewriteHistoryResponse
    {
        public IEnumerable<QualifiedHistory> AllValues { get; set; }
    }
}