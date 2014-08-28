using System;

namespace BLocal.Core
{
    /// <summary>
    /// Represents a locale (language/culture/...)
    /// </summary>
    public class Locale
    {
        public Locale(String name)
        {
            if(name == null)
                throw new ArgumentException("name");
            Name = name;
        }

        public readonly String Name;

        public override string ToString()
        {
            return Name;
        }

        protected bool Equals(Locale other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Locale) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
