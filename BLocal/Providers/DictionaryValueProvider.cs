using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Provides localization based on a dictionnary containing localized values
    /// </summary>
    public class DictionaryValueProvider : ILocalizedValueProvider
    {
        public Dictionary<Qualifier, String> Values { get; set; }

        public DictionaryValueProvider()
        {
            Values = new Dictionary<Qualifier, String>();
        }

        public String GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            try
            {
                return Values[qualifier];
            }
            catch
            {
                Values[qualifier] = defaultValue;
                return defaultValue;
            }
        }

        public void SetValue(Qualifier.Unique qualifier, String value)
        {
            Values[qualifier] = value;
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            try
            {
                return new QualifiedValue(qualifier, Values[qualifier]);
            }
            catch
            {
                var qualifiedValue = new QualifiedValue(qualifier, defaultValue);
                Values[qualifier] = qualifiedValue.Value;
                return qualifiedValue;
            }
        }

        public void Reload()
        {
        }
    }

}
