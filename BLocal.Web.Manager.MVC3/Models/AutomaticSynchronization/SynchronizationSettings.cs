using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class SynchronizationSettings
    {
        public bool Execute { get; set; }
        public String LeftProviderPairName { get; set; }
        public String RightProviderPairName { get; set; }
        public MissingResolutionStrategy LeftMissingStrategy { get; set; }
        public MissingResolutionStrategy RightMissingStrategy { get; set; }
        public DifferingResolutionStrategy DifferingStrategy { get; set; }

        public enum MissingResolutionStrategy
        {
            Audit,
            CopyNew,
            DeleteExisting,
            Ignore,
            ShowConflict
        }

        public enum DifferingResolutionStrategy
        {
            Audit,
            UseLeft,
            UseRight,
            Ignore,
            ShowConflict
        }
    }
}