using System;

namespace BLocal.Core
{
    /// <summary>
    /// Provides the reading and updating of localized values.
    /// </summary>
    public interface ILocalizedValueProvider
    {
        /// <summary>
        /// When implemented, returns the value for the specified qualifier
        /// </summary>
        /// <param name="qualifier">Unique qualifier for which to set the value</param>
        /// <returns></returns>
        String GetValue(Qualifier.Unique qualifier);

        /// <summary>
        /// When implemented, sets the value for a specified qualifier. If the value does not exist, nothing happens.
        /// </summary>
        /// <param name="qualifier">Unique qualifier for which to get set value</param>
        /// <param name="value">Value to set for the qualifier</param>
        void SetValue(Qualifier.Unique qualifier, String value);

        /// <summary>
        /// When implemented, returns the Fully Qualified value for a specified qualifier
        /// </summary>
        /// <param name="qualifier">Unique qualifier</param>
        /// <returns></returns>
        QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier);

        /// <summary>
        /// When implemented, un-caches any cached values
        /// </summary>
        void Reload();
    }
}
