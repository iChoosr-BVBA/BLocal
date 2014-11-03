using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class Creation
    {
        public String ProviderGroupName { get; set; }
        public String Qualifier { get; set; }
        public String NewValue { get; set; }

        public Creation(String providerGroupName, String qualifier, String newValue)
        {
            ProviderGroupName = providerGroupName;
            Qualifier = qualifier;
            NewValue = newValue;
        }
    }
}