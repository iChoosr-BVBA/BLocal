using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class RemoteAccessRequest
    {
        public Guid ApiKey { get; set; }
        public String ProviderGroupName { get; set; }
        public String RequestData { get; set; }
    }
}