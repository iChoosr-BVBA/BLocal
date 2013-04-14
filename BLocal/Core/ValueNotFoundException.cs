using System;

namespace BLocal.Core
{
    /// <summary>
    /// This exception is thrown when a value cannot be found. Typically thrown by any ValueProvider
    /// </summary>
    public class ValueNotFoundException : Exception
    {
        /// <summary>
        /// Qualifier that failed to fetch the value
        /// </summary>
        public Qualifier Qualifier { get; set; }

        public ValueNotFoundException(Qualifier qualifier)
            : this(qualifier, null)
        {
            Qualifier = qualifier;
        }

        public ValueNotFoundException(Qualifier qualifier, Exception innerException)
            : base("Key {" + qualifier.Key + "} not found in {" + qualifier.Part + "} for locale {" + qualifier.Locale + "}", innerException)
        {
            Qualifier = qualifier;
        }
    }
}
