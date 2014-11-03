using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class Update
    {
        public String ProviderGroupName { get; set; }
        public String Qualifier { get; set; }
        public String OldValue { get; set; }
        public String NewValue { get; set; }

        public Update(String providerGroupName, String qualifier, String oldValue, String newValue)
        {
            ProviderGroupName = providerGroupName;
            Qualifier = qualifier;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}