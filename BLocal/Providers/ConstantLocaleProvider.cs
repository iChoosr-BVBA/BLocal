using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Providers
{
    public class ConstantLocaleProvider : ILocaleProvider
    {
        private readonly Locale _locale;

        public ConstantLocaleProvider(Locale locale)
        {
            if(locale == null)
                throw new ArgumentNullException("locale");
            _locale = locale;
        }

        public Locale GetCurrentLocale()
        {
            return _locale;
        }

        public IEnumerable<Locale> GetAvailableLocales()
        {
            yield return _locale;
        }
    }
}
