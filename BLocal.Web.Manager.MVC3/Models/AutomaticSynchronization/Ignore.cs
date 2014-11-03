using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class Ignore
    {
        public String ProviderGroupName { get; set; }
        public String Qualifier { get; set; }

        public Ignore(String providerGroupName, String qualifier)
        {
            ProviderGroupName = providerGroupName;
            Qualifier = qualifier;
        }
    }
}