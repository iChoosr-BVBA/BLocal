using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class Conflict
    {
        public String ProviderGroupName { get; set; }
        public String Qualifier { get; set; }

        public Conflict(String providerGroupName, String qualifier)
        {
            ProviderGroupName = providerGroupName;
            Qualifier = qualifier;
        }
    }
}