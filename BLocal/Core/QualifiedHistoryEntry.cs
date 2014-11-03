using System;

namespace BLocal.Core
{
    public class QualifiedHistoryEntry
    {
        public String Author { get; set; }
        public DateTime DateTime { get; set; }
        public String ContentHash { get; set; }
        public String Content { get; set; }

        public override string ToString()
        {
            return String.Format("{0} | {1} | {2} | {3}", ContentHash, DateTime, Author, Content);
        }
    }
}
