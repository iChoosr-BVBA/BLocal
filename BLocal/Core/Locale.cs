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

        public bool Equals(Locale other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Locale);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
