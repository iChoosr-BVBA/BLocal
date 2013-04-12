using System;
using System.Collections.Generic;

namespace BLocal.Core
{
    /// <summary>
    /// Provides management of localized values (creating, reading, updating, deleting)
    /// </summary>
    public interface ILocalizedValueManager : ILocalizedValueProvider
    {
        /// <summary>
        /// When implemented, tries to update a localized value. If that doesn't work, tries to create it
        /// </summary>
        /// <param name="value">the value to create</param>
        void UpdateCreateValue(QualifiedValue value);

        /// <summary>
        /// When implemented, creates a new localized value
        /// </summary>
        /// <param name="qualifier">the qualifier to create</param>
        /// <param name="value">the value to set for this qualifier</param>
        void CreateValue(Qualifier.Unique qualifier, String value);

        /// <summary>
        /// When implemented, returns all fully qualified values
        /// </summary>
        /// <returns></returns>
        IEnumerable<QualifiedValue> GetAllValuesQualified();

        /// <summary>
        /// When implemented, removes a value completely from the system
        /// </summary>
        /// <param name="qualifier"></param>
        void DeleteValue(Qualifier.Unique qualifier);

        /// <summary>
        /// When implemented, removes all values of all localizations, for a given part / key combination, from the system
        /// </summary>
        /// <param name="part">The part for which to remove all translations</param>
        /// <param name="key">The key for which to remove all translations</param>
        void DeleteLocalizationsFor(Part part, string key);
    }
}
