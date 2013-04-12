using System;
using System.Collections.Generic;

namespace BLocal.Core
{
    public interface ILocalizationLogger
    {
        void Log(Qualifier.Unique accessedQualifier);
        IEnumerable<Log> GetLogsBetween(DateTime start, DateTime end);
        IDictionary<Qualifier.Unique, Log> GetLatestLogsBetween(DateTime start, DateTime end);
    }
}
