using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class RewriteHistoryRequest : IRequest<RewriteHistoryResponse>
    {
        public string Path { get { return "RewriteHistory"; } }

        public QualifiedHistory[] History { get; set; }
    }
}