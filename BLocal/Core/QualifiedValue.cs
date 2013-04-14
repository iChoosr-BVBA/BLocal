using System;

namespace BLocal.Core
{
    /// <summary>
    /// Represents a qualifier and its corresponding value
    /// </summary>
    public class QualifiedValue
    {
        public Qualifier.Unique Qualifier { get; set; }
        public Value Value { get; set; }

        public QualifiedValue() {}

        public QualifiedValue(Qualifier.Unique qualifier, Value value)
        {
            Qualifier = qualifier;
            Value = value;
        }

        public override String ToString()
        {
            return String.Format("{0} {1}: {2}", Qualifier, Value.ContentType, Value.Content);
        }

        public bool Equals(QualifiedValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Qualifier, Qualifier) && Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (QualifiedValue)) return false;
            return Equals((QualifiedValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Qualifier != null ? Qualifier.GetHashCode() : 0)*397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
