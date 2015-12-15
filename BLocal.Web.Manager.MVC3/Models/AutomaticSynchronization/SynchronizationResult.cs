using System;
using System.Collections.Generic;

namespace BLocal.Web.Manager.Models.AutomaticSynchronization
{
    public class SynchronizationResult
    {
        public readonly List<Creation> Created = new List<Creation>();
        public readonly List<Removal> Removed = new List<Removal>();
        public readonly List<Update> Updated = new List<Update>();
        public readonly List<Ignore> Ignored = new List<Ignore>();
        public readonly List<Conflict> Unresolved = new List<Conflict>();
        public readonly List<String> Problems = new List<string>();
    }
}