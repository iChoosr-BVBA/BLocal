using System.Collections.Generic;

namespace BLocal.Core
{
    /// <summary>
    /// Provides functionality of getting and setting locales
    /// </summary>
    public interface ILocaleProvider
    {
        /// <summary>
        /// When implemented, gets the current locale.
        /// </summary>
        /// <returns></returns>
        Locale GetCurrentLocale();

        /// <summary>
        /// When implemented, gets a list of all available locales.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Locale> GetAvailableLocales();
    }
}
