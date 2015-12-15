using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Core;

namespace BLocal.Providers
{
    public class VoidHistoryManager : ILocalizationHistoryManager
    {
        public IEnumerable<QualifiedHistory> ProvideHistory()
        {
            return Enumerable.Empty<QualifiedHistory>();
        }

        public IEnumerable<QualifiedValue> FindValuesConflictingWithHistory(IEnumerable<QualifiedValue> currentValues)
        {
            return Enumerable.Empty<QualifiedValue>();
        }

        public void RewriteHistory(IEnumerable<QualifiedHistory> newHistory)
        {
        }

        public void Persist()
        {
        }

        public void Reload()
        {
        }

        public void ProgressHistory(QualifiedValue value, String author)
        {
            
        }

        public QualifiedHistory GetHistory(Qualifier.Unique qualifier)
        {
            return new QualifiedHistory { Qualifier = qualifier };
        }

        public void OverrideHistory(QualifiedHistory qualifiedHistory)
        {
        }
    }
}
