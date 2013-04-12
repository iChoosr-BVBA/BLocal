using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using BLocal.Core;

namespace BLocal.Web
{
    public class MvcLocaleProvider : ILocaleProvider
    {
        private readonly String[] _locales;


        public MvcLocaleProvider(params String[] acceptedLocales)
        {
            _locales = acceptedLocales;
        }

        public Locale GetCurrentLocale()
        {
            try {
                var browserLang = HttpContext.Current.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"].ToLowerInvariant();
                var acceptableBrowserLang = browserLang.Split(',', ';', '-')
                    .FirstOrDefault(lang => _locales.Contains(lang));
                if (!string.IsNullOrWhiteSpace(acceptableBrowserLang))
                    return new Locale(acceptableBrowserLang);
            }
            catch (NullReferenceException) { }

            var threadLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (_locales.Contains(threadLang))
                return new Locale(threadLang);

            return new Locale(_locales.First());
        }

        public IEnumerable<Locale> GetAvailableLocales()
        {
            return _locales.Select(locale => new Locale(locale));
        }
    }
}