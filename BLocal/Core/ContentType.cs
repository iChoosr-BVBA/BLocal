using System;
using System.Collections.Generic;

namespace BLocal.Core
{
    public class ContentType
    {
        private static readonly Dictionary<String, ContentType> ContentTypes = new Dictionary<String, ContentType>();
        public static ContentType Find(String name)
        {
            try
            {
                return ContentTypes[name];
            }
            catch(KeyNotFoundException)
            {
                return Unknown;
            }
        }
        public static IEnumerable<ContentType> All { get { return ContentTypes.Values; } }

        public static ContentType Unknown = Create("Unknown", value => value);
        public static ContentType Text = Create("Text", value => value);

        public static ContentType Create(String name, Func<String, String> decoder)
        {
            var newType = new ContentType(name, decoder);
            ContentTypes.Add(name, newType);
            return newType;
        }

        public static ContentType Spoof(String name)
        {
            var type = Find(name);
            if(type == Unknown)
                type = new ContentType(name, value => value);
            return type;
        }

        public String Name { get; private set; }
        private readonly Func<String, String> _decoder;

        private ContentType(String name, Func<String, String> decoder)
        {
            Name = name;
            _decoder = decoder;
        }

        public String Decode(String value)
        {
            return _decoder(value);
        }

        public override string ToString()
        {
            return String.Format("({0})", Name);
        }

        protected bool Equals(ContentType other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ContentType);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
