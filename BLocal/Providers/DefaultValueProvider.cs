using System;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Provides localization based on whatever default value it receives
    /// </summary>
    public class DefaultValueProvider : ILocalizedValueProvider
    {
        public string GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            if (defaultValue == null)
                throw new ValueNotFoundException(qualifier);
            return defaultValue;
        }

        public void SetValue(Qualifier.Unique qualifier, String value)
        {
            throw  new NotImplementedException("Default Value Provider always returns the default value, this cannot be changed!");
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            if(defaultValue == null)
                throw new ValueNotFoundException(qualifier);

            return new QualifiedValue(qualifier, new Value(ContentType.Unknown, defaultValue));
        }

        public void Reload()
        {
        }
    }

}
