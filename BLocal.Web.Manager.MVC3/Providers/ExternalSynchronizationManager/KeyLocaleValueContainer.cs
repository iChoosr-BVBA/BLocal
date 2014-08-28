using System;
using System.Collections.Generic;
using System.Linq;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class KeyLocaleValueContainer
    {
        private readonly Dictionary<String, LocaleValueContainer> _keyLocales = new Dictionary<String, LocaleValueContainer>();

        public LocaleValueContainer GetLocaleValueContainerFor(String key)
        {
            return _keyLocales[key];
        }

        public void SetValueForKeyAndLocale(String key, String locale, String value, Boolean createIfNovel)
        {
            if (!_keyLocales.ContainsKey(key))
            {
                if (!createIfNovel)
                    return;
                _keyLocales[key] = new LocaleValueContainer();
            }
            _keyLocales[key].SetValueForLocale(locale, value);
        }

        public void DeleteValueForKeyAndLocale(String key, String locale)
        {
            if (_keyLocales.ContainsKey(key))
                _keyLocales[key].DeleteValueForLocale(locale);
        }

        public void DeleteValuesForKey(String key)
        {
            if (_keyLocales.ContainsKey(key))
                _keyLocales.Remove(key);
        }

        public IEnumerable<KeyValuePair<String, LocaleValueContainer>> GetAllKeyBasedLocales()
        {
            return _keyLocales.ToArray();
        }
    }
}