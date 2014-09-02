using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class SetAuditsRequest : IRequest<SetAuditsResponse>
    {
        public string Path { get { return "SetAudits"; } }

        public LocalizationAudit[] Audits { get; set; }
    }
}