using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ExternalSynchronizationRequest
    {
        public Guid ApiKey { get; set; }
        public String ProviderGroupName { get; set; }
        public String RequestData { get; set; }
    }
}