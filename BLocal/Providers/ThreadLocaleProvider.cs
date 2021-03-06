﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Attempts to retrieve the locale from the current thread
    /// </summary>
    public class ThreadLocaleProvider : ILocaleProvider
    {
        public Locale GetCurrentLocale()
        {
            return new Locale((Thread.CurrentThread.CurrentUICulture).IetfLanguageTag);
        }

        public IEnumerable<Locale> GetAvailableLocales()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => new Locale(culture.IetfLanguageTag));
        }
    }
}
