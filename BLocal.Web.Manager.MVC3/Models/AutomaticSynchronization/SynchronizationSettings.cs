using System;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class SynchronizationSettings
    {
        public bool Execute { get; set; }
        public String LeftProviderGroupName { get; set; }
        public String RightProviderGroupName { get; set; }
        public String LeftAuthorName { get; set; }
        public String RightAuthorName { get; set; }
        public MissingResolutionStrategy LeftMissingStrategy { get; set; }
        public MissingResolutionStrategy RightMissingStrategy { get; set; }
        public DifferingResolutionStrategy DifferingStrategy { get; set; }

        public enum MissingResolutionStrategy
        {
            History,
            CopyNew,
            DeleteExisting,
            Ignore,
            ShowConflict
        }

        public enum DifferingResolutionStrategy
        {
            History,
            UseLeft,
            UseRight,
            Ignore,
            ShowConflict
        }
    }
}