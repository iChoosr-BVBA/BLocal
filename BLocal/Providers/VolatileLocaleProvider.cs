using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Providers
{
    public class VolatileLocaleProvider : ILocaleProvider
    {
        public Locale CurrentLocale { get; set; }

        public Locale GetCurrentLocale()
        {
            return CurrentLocale;
        }

        public IEnumerable<Locale> GetAvailableLocales()
        {
            throw new NotImplementedException();
        }
    }
}
