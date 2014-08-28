using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class ReloadRequest : IRequest<FullContentResponse>
    {
        public String Path { get { return "Reload"; } }
    }
}