using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Providers;

namespace BLocal.Core
{
    /// <summary>
    /// A Facade class for easy getting and setting of values.
    /// </summary>
    public class LocalizationRepository
    {
        /// <summary>
        /// Makes it easy to create multiple repositories with variance in different providers
        /// </summary>
        public class Factory
        {
            private readonly ILocalizedValueProvider _valueProvider;
            private readonly INotifier _notifier;
            private readonly ILocaleProvider _localeProvider;

            public Factory(ILocalizedValueProvider valueProvider, INotifier notifier, ILocaleProvider localeProvider)
            {
                _valueProvider = valueProvider;
                _notifier = notifier;
                _localeProvider = localeProvider;
            }

            public LocalizationRepository CreateRepository(IPartProvider partProvider)
            {
                return new LocalizationRepository(_valueProvider, _notifier, _localeProvider, partProvider);
            }

            public LocalizationRepository CreateRepository(IPartProvider partProvider, ILocalizationLogger logger)
            {
                return new LocalizationRepository(_valueProvider, _notifier, _localeProvider, partProvider, logger);
            }
        }

        public ILocalizedValueProvider Values { get; private set; }
        public INotifier Notifier { get; private set; }
        public ILocalizationLogger Logger { get; private set; }

        public ILocaleProvider Locales { get; set; }
        public IPartProvider Parts { get; set; }

        public Part DefaultPart
        {
            get { return Parts.GetCurrentPart(); }
        }
        public Locale DefaultLocale
        {
            get { return Locales.GetCurrentLocale(); }
        }

        public LocalizationRepository(ILocalizedValueProvider valueProvider, INotifier notifier, ILocaleProvider localeProvider, IPartProvider partProvider, ILocalizationLogger logger = null)
        {
            Values = valueProvider;
            Notifier = notifier;
            Locales = localeProvider;
            Parts = partProvider;
            Logger = logger ?? new VoidLogger();
        }

        /// <summary>
        /// Gets the current value for a specified key, overriding the current part and locale
        /// </summary>
        /// <param name="qualifier">Qualifier for the value to get. Should have at least its key set</param>
        /// <param name="defaultValue">If the localization does not yet exist, create with default value</param>
        /// <returns></returns>
        public String Get(Qualifier.WithKey qualifier, String defaultValue = null)
        {
            var value = GetQualified(qualifier, defaultValue);
            return value == null ? String.Empty : value.Value;
        }

        /// <summary>
        /// Gets the current spec for a specified key, overriding the current part and locale
        /// </summary>
        /// <param name="qualifier">Qualifier to get specs for, should at least contain the key.</param>
        /// <param name="defaultValue">If the localization does not yet exist, create with default value</param>
        /// <returns></returns>
        public QualifiedValue GetQualified(Qualifier.WithKey qualifier, String defaultValue = null)
        {
            var locale = qualifier.Locale ?? Locales.GetCurrentLocale();
            var part = qualifier.Part ?? Parts.GetCurrentPart();
            var resultQualifier = new Qualifier.Unique(part, locale, qualifier.Key);

            try {
                var value = Values.GetQualifiedValue(resultQualifier, defaultValue);
                if (value == null)
                    throw new ValueNotFoundException(qualifier);
                Logger.Log(value.Qualifier);
                return value;
            }
            catch (ValueNotFoundException) {
                Notifier.NotifyMissing(resultQualifier);
                return new QualifiedValue(resultQualifier, defaultValue ?? String.Format("[{0}]", qualifier.Key));
            }
        }

        /// <summary>
        /// Gets the value for a single key and returns it
        /// </summary>
        /// <param name="key">Key to get the value of</param>
        /// <param name="defaultValue">If the localization does not yet exist, create with default value</param>
        /// <returns></returns>
        public String Get(String key, String defaultValue = null)
        {
            return Get(new Qualifier.Unique(Parts.GetCurrentPart(), Locales.GetCurrentLocale(), key), defaultValue);
        }
        /// <summary>
        /// Gets multiple keys and returns them as a key => value dictionary using the current part and locale
        /// </summary>
        /// <param name="keys">List of keys to get the values of</param>
        /// <returns></returns>
        public Dictionary<String, String> Get(params String[] keys)
        {
            return keys.ToDictionary(key => key, key => Get(new Qualifier.WithKey(key)));
        }
        /// <summary>
        /// Gets values for all locales for the current part and returns them as a key => value dictionary 
        /// </summary>
        /// <param name="key">Key of which to get values from all locales from</param>
        /// <returns></returns>
        public Dictionary<Locale, String> GetAllFor(String key)
        {
            return Locales.GetAvailableLocales().ToDictionary(locale => locale, locale => Get(new Qualifier.WithKey(key) { Locale = locale}));
        }

        /// <summary>
        /// Sets the value for a given qualifier
        /// </summary>
        /// <param name="qualifier">Qualifies the value to be set</param>
        /// <param name="value">The new value to override the old value with</param>
        public void Set(Qualifier.WithKey qualifier, String value)
        {
            var locale = qualifier.Locale ?? Locales.GetCurrentLocale();
            var part = qualifier.Part ?? Parts.GetCurrentPart();

            Values.SetValue(new Qualifier.Unique(part, locale, qualifier.Key), value);
        }
    }
}
