using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class BasicRequest : IRequest<Object>
    {
        public string Path { get; set; }
    }
}