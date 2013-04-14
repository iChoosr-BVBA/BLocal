using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Locale Provider whose Locale can be set at any given time.
    /// </summary>
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
