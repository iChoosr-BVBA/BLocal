using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Offers only select cultures
    /// Will provide default culture if thread culture is not compatible
    /// Will synchronize thread culture with retrieved culture
    /// </summary>
    public class CulturedThreadLocaleProvider : ILocaleProvider
    {
        private readonly CultureInfo[] _cultures;
        private readonly CultureInfo _defaultCulture;

        public CulturedThreadLocaleProvider(params CultureInfo[] culturesAvailable)
        {
            _cultures = culturesAvailable;
            _defaultCulture = culturesAvailable.FirstOrDefault();
        }

        public CulturedThreadLocaleProvider(IEnumerable<CultureInfo> culturesAvailable, CultureInfo defaultCulture = null)
            : this(culturesAvailable.ToArray())
        {
            _defaultCulture = defaultCulture ?? _defaultCulture;
        }

        public Locale GetCurrentLocale()
        {
            return new Locale(GetCurrentCulture().IetfLanguageTag);
        }

        public CultureInfo GetCurrentCulture()
        {
            var threadCulture = Thread.CurrentThread.CurrentUICulture;

            if (_cultures.Contains(threadCulture))
                return threadCulture;
            if (_cultures.Contains(threadCulture.Parent))
                return threadCulture.Parent;

            Thread.CurrentThread.CurrentCulture = _defaultCulture;
            Thread.CurrentThread.CurrentUICulture = _defaultCulture;
            return _defaultCulture;
        }

        public IEnumerable<Locale> GetAvailableLocales()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => new Locale(culture.IetfLanguageTag));
        }
    }
}
