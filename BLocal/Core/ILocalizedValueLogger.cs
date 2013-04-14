using System;
using System.Collections.Generic;

namespace BLocal.Core
{
    /// <summary>
    /// Provides functionality that logs every time a localized value is used. Use with care!
    /// </summary>
    public interface ILocalizationLogger
    {
        /// <summary>
        /// Logs a value that was accessed
        /// </summary>
        /// <param name="accessedQualifier">Qualifier of the vlalue that was accessed</param>
        void Log(Qualifier.Unique accessedQualifier);
        /// <summary>
        /// Retrieves all logs for a period
        /// </summary>
        /// <param name="start">Date on which to start checking (inclusive)</param>
        /// <param name="end">Date on which to end checking (inclusive)</param>
        /// <returns></returns>
        IEnumerable<Log> GetLogsBetween(DateTime start, DateTime end);
        /// <summary>
        /// Returns the most recent time of useage for every qualifier in a certain period
        /// </summary>
        /// <param name="start">Date on which to start checking (inclusive)</param>
        /// <param name="end">Date on which to end checking (inclusive)</param>
        /// <returns></returns>
        IDictionary<Qualifier.Unique, Log> GetLatestLogsBetween(DateTime start, DateTime end);
    }
}
