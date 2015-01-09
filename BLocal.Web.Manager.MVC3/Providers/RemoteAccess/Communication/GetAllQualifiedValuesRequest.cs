using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class ReloadRequest : IRequest<FullContentResponse>
    {
        public String Path { get { return "Reload"; } }
    }
}