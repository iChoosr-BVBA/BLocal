using System.Collections.Generic;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class ProcessBatchRequest : IRequest<ProcessBatchResponse>
    {
        public string Path { get { return "ProcessBatch"; } }
        public List<RemoteAccessRequest> Requests { get; set; }
    }
}