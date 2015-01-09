using System;
using System.Collections.Generic;
using BLocal.Web.Manager.Models.ExternalSynchronization;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ProcessBatchRequest : IRequest<ProcessBatchResponse>
    {
        public string Path { get { return "ProcessBatch"; } }
        public List<ExternalSynchronizationRequest> Requests { get; set; }
    }
}