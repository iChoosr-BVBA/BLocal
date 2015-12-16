using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class HistoryChecker
    {
        private const string HistoryConflictMessage = "Provider {0} values on disk do not correspond with recorded history, aborting due to possible file corruption. To fix this, see \"View History\" on the home page. Corrupted values: {1}";

        public IEnumerable<QualifiedConflict> FindValuesConflictingWithHistory(IEnumerable<QualifiedValue> qualifiedValues, IEnumerable<QualifiedHistory> history)
        {
            var qualifiedValueDictionary = qualifiedValues.ToDictionary(v => v.Qualifier);
            var historyDictionary = history.ToDictionary(h => h.Qualifier);

            foreach (var historyEntry in historyDictionary.Values)
            {
                var valueWasDeletedInHistory = historyEntry.LatestEntry() == null || historyEntry.LatestEntry().Content == null;
                var valueExistsInValues = qualifiedValueDictionary.ContainsKey(historyEntry.Qualifier);
               
                // check for newly deleted values
                if (valueWasDeletedInHistory && valueExistsInValues)
                {
                    yield return new QualifiedConflict(historyEntry.Qualifier, null, qualifiedValueDictionary[historyEntry.Qualifier].Value);
                    continue;
                }

                // check for newly undeleted values
                if (!valueWasDeletedInHistory && !valueExistsInValues)
                {
                    yield return new QualifiedConflict(historyEntry.Qualifier, historyEntry.LatestEntry().Content, null);
                    continue;
                }

                // nothing to compare with
                if (valueWasDeletedInHistory)
                    continue;

                // check for content modifications
                var qualifiedValue = qualifiedValueDictionary[historyEntry.Qualifier];
                if (qualifiedValue.Value != historyEntry.LatestEntry().Content)
                    yield return new QualifiedConflict(qualifiedValue.Qualifier, historyEntry.LatestEntry().Content, qualifiedValue.Value);
            }

            // check for newly created values
            foreach (var qualifiedValue in qualifiedValueDictionary.Values)
            {
                if (!historyDictionary.ContainsKey(qualifiedValue.Qualifier))
                    yield return new QualifiedConflict(qualifiedValue.Qualifier, null, qualifiedValue.Value);
            }
        }

        public String ProvideConflictMessage(String providerConfigName, IEnumerable<QualifiedConflict> conflictingValues)
        {
            return String.Format(HistoryConflictMessage, providerConfigName, String.Join(", ", conflictingValues.Select(c => c.Qualifier.ToString())));
        }

        public void ValidateHistory(IEnumerable<QualifiedValue> allValues, IEnumerable<QualifiedHistory> history, String providerName)
        {
            var conflicts = FindValuesConflictingWithHistory(allValues, history).ToArray();
            if (!conflicts.Any())
                return;

            var message = ProvideConflictMessage(providerName, conflicts);
            throw new HistoryConflictException(message);
        }

        public class HistoryConflictException : Exception
        {
            public HistoryConflictException(string message) : base(message)
            {
                
            }
        }

        public class QualifiedConflict
        {
            public Qualifier.Unique Qualifier { get; private set; }
            public String LatestHistoryValue { get; private set; }
            public String CurrentValue { get; private set; }

            public QualifiedConflict(Qualifier.Unique qualifier, string latestHistoryValue, string currentValue)
            {
                Qualifier = qualifier;
                LatestHistoryValue = latestHistoryValue;
                CurrentValue = currentValue;
            }
        }
    }
}