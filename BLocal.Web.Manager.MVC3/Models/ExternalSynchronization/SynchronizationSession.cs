using System;
using System.Collections.Generic;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.ExternalSynchronization
{
    public class SynchronizationSession
    {
        public DateTime StartDateTime { get; private set; }
        public readonly Dictionary<String, ProviderGroup> ProviderGroups = new Dictionary<string, ProviderGroup>();

        public SynchronizationSession()
        {
            StartDateTime = DateTime.Now;
        }
    }
}