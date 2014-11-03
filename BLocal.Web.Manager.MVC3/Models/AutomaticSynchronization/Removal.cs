using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class Removal
    {
        public String ProviderGroupName { get; set; }
        public String Qualifier { get; set; }
        public String OldValue { get; set; }

        public Removal(String providerGroupName, String qualifier, String oldValue)
        {
            ProviderGroupName = providerGroupName;
            Qualifier = qualifier;
            OldValue = oldValue;
        }
    }
}