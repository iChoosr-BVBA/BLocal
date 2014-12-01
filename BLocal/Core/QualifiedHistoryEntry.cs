using System;

namespace BLocal.Core
{
    public class QualifiedHistoryEntry
    {
        public String Author { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public String ContentHash { get; set; }
        public String Content { get; set; }

        public override string ToString()
        {
            return String.Format("{0} | {1} | {2} | {3}", ContentHash, DateTimeUtc, Author, Content);
        }

        protected bool Equals(QualifiedHistoryEntry other)
        {
            return DateTimeUtc.Equals(other.DateTimeUtc) && String.Equals(ContentHash, other.ContentHash);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QualifiedHistoryEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DateTimeUtc.GetHashCode()*397) ^ (ContentHash != null ? ContentHash.GetHashCode() : 0);
            }
        }
    }
}
