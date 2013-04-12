using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Core;

namespace BLocal.Providers
{
    public class VoidLogger : ILocalizationLogger
    {
        public void Log(Qualifier.Unique accessedQualifier)
        {
        }

        public IEnumerable<Log> GetLogsBetween(DateTime start, DateTime end)
        {
            return Enumerable.Empty<Log>();
        }

        public IDictionary<Qualifier.Unique, Log> GetLatestLogsBetween(DateTime start, DateTime end)
        {
            return new Dictionary<Qualifier.Unique, Log>();
        }
    }
}
