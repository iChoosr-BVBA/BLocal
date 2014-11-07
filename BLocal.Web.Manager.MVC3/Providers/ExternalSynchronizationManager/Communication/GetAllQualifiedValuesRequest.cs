using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ReloadRequest : IRequest<FullContentResponse>
    {
        public String Path { get { return "Reload"; } }
    }
}