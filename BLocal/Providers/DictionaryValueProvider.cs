using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Providers
{
    public class DictionaryValueProvider : ILocalizedValueProvider
    {
        public Dictionary<Qualifier, Value> Values { get; set; }

        public DictionaryValueProvider()
        {
            Values = new Dictionary<Qualifier, Value>();
        }

        public string GetValue(Qualifier.Unique qualifier)
        {
            try
            {
                return Values[qualifier].DecodedContent;
            }
            catch
            {
                throw new ValueNotFoundException(qualifier);
            }
        }

        public void SetValue(Qualifier.Unique qualifier, String value)
        {
            Values[qualifier].Content = value;
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier)
        {
            try
            {
                return new QualifiedValue(qualifier, Values[qualifier]);
            }
            catch
            {
                throw new ValueNotFoundException(qualifier);
            }
        }

        public void Reload()
        {
        }
    }

}
