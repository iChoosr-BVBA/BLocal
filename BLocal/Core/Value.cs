using System;
using System.Collections.Generic;
using System.Linq;

namespace BLocal.Core
{
    /// <summary>
    /// Represents a localized value
    /// </summary>
    public class Value
    {
        public ContentType ContentType { get; set; }
        public String Content
        {
            get { return _content; }
            set {
                _content = value;
                _decodedContent = null;
            }
        }

        private String _content;
        private String _decodedContent;

        /// <summary>
        /// Returns content as decoded by its content type. Decoded value is then cached as long as the Value object lives.
        /// </summary>
        public String DecodedContent
        {
            get { return _decodedContent ?? (_decodedContent = ContentType.Decode(Content)); }
        }

        /// <summary>
        /// Makes replacements in the value BEFORE running it through its contenttyp's decoder (not usually the way you'd want to do it)
        /// </summary>
        /// <param name="replacements">Replacements to make in the value before decoding it</param>
        /// <returns></returns>
        public String DecodeWithReplacements(IEnumerable<KeyValuePair<String, String>> replacements)
        {
            return ContentType.Decode(replacements.Aggregate(_content, (current, replacement) => current.Replace(replacement.Key, replacement.Value)));
        }

        public Value()
        {
        }

        public Value(ContentType contentType, string content)
        {
            ContentType = contentType;
            Content = content;
        }

        public override string ToString()
        {
            return String.Format("{{{0}:{1}}}", ContentType, Content);
        }

        public bool Equals(Value other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._content, _content) && Equals(other.ContentType, ContentType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Value);
        }

        // ReSharper disable NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            unchecked {
                return ((_content != null ? _content.GetHashCode() : 0)*397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
            }
        }
        // ReSharper restore NonReadonlyFieldInGetHashCode
    }
}
