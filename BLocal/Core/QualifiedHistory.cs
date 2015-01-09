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
            var latest = LatestEntry();

            // skip the latest entry
            return other.Entries.Take(other.Entries.Count - 1).Any(entry =>
                entry.ContentHash == latest.ContentHash
                && entry.DateTimeUtc == latest.DateTimeUtc
                && entry.Author == latest.Author
            );
        }
    }
}
