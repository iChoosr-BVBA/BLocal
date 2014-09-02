using System;
using System.Collections.Generic;
using System.Linq;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class LocaleValueContainer
    {
        private readonly Dictionary<String, String> _localeValues = new Dictionary<String, String>();

        public string GetValueFor(String locale)
        {
            if (_localeValues.ContainsKey(locale))
                return _localeValues[locale];

            throw new KeyNotFoundException("Key found but has no value for this locale");
        }

        public void SetValueForLocale(String locale, String value)
        {
            _localeValues[locale] = value;
        }

        public void DeleteValueForLocale(String locale)
        {
            if (_localeValues.ContainsKey(locale))
                _localeValues.Remove(locale);
        }

        public IEnumerable<KeyValuePair<String, String>> GetAllLocaleBasedValues()
        {
            return _localeValues.ToArray();
        }
    }
}