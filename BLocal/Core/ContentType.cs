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

        public static ContentType Unknown = Create("Unknown", null);
        public static ContentType Unspecified = Create("Unspecified", null);
        public static ContentType Text = Create("Text", null);
        public static ContentType Html = Create("Html", null);

        /// <summary>
        /// Creates a new content type and adds it to the list
        /// </summary>
        /// <param name="name">Name of the content type</param>
        /// <param name="decoder">Any content retrieved will first be decoded. Null returns normal value.</param>
        /// <returns></returns>
        public static ContentType Create(String name, Func<String, String> decoder)
        {
            var newType = new ContentType(name, decoder);
            ContentTypes.Add(name, newType);
            return newType;
        }
        
        /// <summary>
        /// Find or pretend to find this content type
        /// </summary>
        /// <param name="name">Name of the content type to find</param>
        /// <returns></returns>
        public static ContentType Spoof(String name)
        {
            var type = Find(name);
            if(Equals(type, Unknown))
                type = new ContentType(name, null);
            return type;
        }

        public String Name { get; private set; }
        private readonly Func<String, String> _decoder;

        private ContentType(String name, Func<String, String> decoder)
        {
            Name = name;
            _decoder = decoder;
        }

        /// <summary>
        /// Returns the decoded value
        /// </summary>
        /// <param name="value">Value to decode</param>
        /// <returns></returns>
        public String Decode(String value)
        {
            return _decoder == null ? value : _decoder(value);
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
