using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class OverrideHistoryRequest : IRequest<OverrideHistoryResponse>
    {
        public String Path { get { return "OverrideHistory"; } }

        public QualifiedHistory History { get; set; }
    }
}