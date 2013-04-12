using System;

namespace BLocal.Core
{
    public class ValueNotFoundException : Exception
    {
        public Qualifier Qualifier { get; set; }

        public ValueNotFoundException(Qualifier qualifier) : this(qualifier, null)
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
