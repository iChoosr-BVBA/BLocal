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
        /// Adjusts history based on new values, adds all found changes as done by author parameter
        /// </summary>
        /// <param name="currentValues"></param>
        /// <param name="author"></param>
        void AdjustHistory(IEnumerable<QualifiedValue> currentValues, String author);

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
    }
}
