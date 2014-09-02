using System;

namespace BLocal.Core
{
    public class LocalizationAudit
    {
        public Qualifier.Unique Qualifier { get; set; }
        public DateTime LatestUpdate { get; set; }
        public String LatestValueHash { get; set; }
        public String PreviousValueHash { get; set; }
    }
}
