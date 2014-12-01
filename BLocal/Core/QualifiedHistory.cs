using System;
using System.Collections.Generic;
using System.Linq;

namespace BLocal.Core
{
    public class QualifiedHistory
    {
        public Qualifier.Unique Qualifier { get; set; }
        public List<QualifiedHistoryEntry> Entries { get; set; }

        public QualifiedHistoryEntry LatestEntry() { return Entries.LastOrDefault(); }
        public QualifiedHistoryEntry PreviousEntry() { return Entries.Count > 1 ? Entries[Entries.Count - 2] : null; }

        public QualifiedHistory()
        {
            Entries = new List<QualifiedHistoryEntry>();
        }

        public Boolean IsPreviousVersionOf(QualifiedHistory other)
        {
            var thisCurrent = LatestEntry();
            var othersPrevious = other.PreviousEntry();

            if (othersPrevious == null)
                return false;

            if (thisCurrent.ContentHash == othersPrevious.ContentHash && thisCurrent.DateTimeUtc == othersPrevious.DateTimeUtc)
                return true;

            // all entries except the latest one, which we already compared, from new to old
            for(var i = Entries.Count - 2; i >= 0; i--) 
            {
                var entry = Entries[i];
                if (entry.ContentHash == thisCurrent.ContentHash && entry.DateTimeUtc == thisCurrent.DateTimeUtc)
                    return true;

                if (entry.DateTimeUtc < thisCurrent.DateTimeUtc)
                    return false;
            }

            return false;
        }
    }
}
