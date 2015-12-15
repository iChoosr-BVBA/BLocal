using System;
using System.Collections.Generic;

namespace BLocal.Core
{
    public interface ILocalizationHistoryManager
    {
        /// <summary>
        /// When implemented, returns all history
        /// </summary>
        /// <returns></returns>
        IEnumerable<QualifiedHistory> ProvideHistory();

        /// <summary>
        /// When implemented, rewrites history from scratch
        /// </summary>
        /// <returns></returns>
        void RewriteHistory(IEnumerable<QualifiedHistory> newHistory);

        /// <summary>
        /// When implemented, save any unsaved changes
        /// </summary>
        void Persist();

        /// <summary>
        /// When implemented, removes history from memory
        /// </summary>
        void Reload();

        /// <summary>
        /// When implemented, adjusts the history to take the changed value into account.
        /// </summary>
        /// <param name="value">Value that has changed</param>
        /// <param name="author">Author for the change</param>
        void ProgressHistory(QualifiedValue value, String author);

        /// <summary>
        /// When implemented, returns the history for a single qualifier
        /// </summary>
        /// <param name="qualifier">The qualifier for which to find history</param>
        /// <returns></returns>
        QualifiedHistory GetHistory(Qualifier.Unique qualifier);

        /// <summary>
        /// When implemented, sets the history for a single qualifier
        /// </summary>
        /// <param name="qualifiedHistory">The qualified history to override with</param>
        void OverrideHistory(QualifiedHistory qualifiedHistory);
    }
}
