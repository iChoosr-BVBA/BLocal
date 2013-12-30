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
        public Dictionary<Qualifier, Value> Values { get; set; }

        public DictionaryValueProvider()
        {
            Values = new Dictionary<Qualifier, Value>();
        }

        public string GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            try
            {
                return Values[qualifier].DecodedContent;
            }
            catch
            {
                Values[qualifier] = new Value(ContentType.Unknown, defaultValue);
                return defaultValue;
            }
        }

        public void SetValue(Qualifier.Unique qualifier, String value)
        {
            Values[qualifier].Content = value;
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            try
            {
                return new QualifiedValue(qualifier, Values[qualifier]);
            }
            catch
            {
                var qualifiedValue = new QualifiedValue(qualifier, new Value(ContentType.Unknown, defaultValue));
                Values[qualifier] = qualifiedValue.Value;
                return qualifiedValue;
            }
        }

        public void Reload()
        {
        }
    }

}
