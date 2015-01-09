using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class BasicRequest : IRequest<Object>
    {
        public string Path { get; private set; }
    }
}