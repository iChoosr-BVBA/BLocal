using System;

namespace BLocal.Core
{
    /// <summary>
    /// Represents the three dimensions in which a value can vary
    /// </summary>
    public class Qualifier
    {
        public Part Part { get; set; }
        public Locale Locale { get; set; }
        public String Key { get; set; }

        public Qualifier() { }

        public Qualifier(Part part, Locale locale, string key)
        {
            Part = part;
            Locale = locale;
            Key = key;
        }

        public override string ToString()
        {
            return string.Format("[{0}.{1}-{2}]", Part, Locale, Key);
        }

        public bool Equals(Qualifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Part, Part) && Equals(other.Locale, Locale) && Equals(other.Key, Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Qualifier;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Part != null ? Part.GetHashCode() : 0);
                result = (result*397) ^ (Locale != null ? Locale.GetHashCode() : 0);
                result = (result*397) ^ (Key != null ? Key.GetHashCode() : 0);
                return result;
            }
        }

        /// <summary>
        /// Represents a qualifier for which at least the key is set.
        /// </summary>
        public class WithKey : Qualifier
        {
            public WithKey(String key)
            {
                if(String.IsNullOrEmpty(key))
                    throw new ArgumentException("Key should not be Null or empty!", "key");
                Key = key;
            }
            public WithKey(String key, Locale locale)
            {
                if (String.IsNullOrEmpty(key))
                    throw new ArgumentException("Key should not be Null or empty!", "key");
                Key = key;
                Locale = locale;
            }
            public WithKey(String key, Part part)
            {
                if (String.IsNullOrEmpty(key))
                    throw new ArgumentException("Key should not be Null or empty!", "key");
                Key = key;
                Part = part;
            }
        }

        /// <summary>
        /// Represents a qualifier for which all three dimensions are set. Forms a unique combination for which only one value can be resolved.
        /// </summary>
        public class Unique : WithKey
        {
            public Unique(Part part, Locale locale, String key) : base(key)
            {
                if(String.IsNullOrEmpty(key))
                    throw new ArgumentException("Key should not be Null or empty!", "key");
                if(part == null)
                    throw new ArgumentException("Part should not be Null!", "part");
                if (locale == null)
                    throw new ArgumentException("Locale should not be Null!", "locale");

                Part = part;
                Locale = locale;
            }
        }
    }
}
