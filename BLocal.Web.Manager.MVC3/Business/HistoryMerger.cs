using System;
using System.Linq;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class HistoryMerger
    {
        public static void MergeHistory(Qualifier.Unique qualifier, ILocalizationHistoryManager winningHistory, ILocalizationHistoryManager losingHistory)
        {
            var winningQualifiedHistory = winningHistory.GetHistory(qualifier) ?? new QualifiedHistory();
            var losingQualifiedHistory = losingHistory.GetHistory(qualifier) ?? new QualifiedHistory();

            var mergedHistory = winningQualifiedHistory.Entries
                .Union(losingQualifiedHistory.Entries).Distinct().OrderBy(h => h.DateTimeUtc)
                .Except(new[] { winningQualifiedHistory.LatestEntry() }).Union(new [] {winningQualifiedHistory.LatestEntry() })
                .Where(e => e != null);

            var qualifiedMergedHistory = new QualifiedHistory {Qualifier = qualifier, Entries = mergedHistory.ToList()};

            winningHistory.OverrideHistory(qualifiedMergedHistory);
            losingHistory.OverrideHistory(qualifiedMergedHistory);
        }

        public static QualifiedHistory MergeHistoryEntries(QualifiedHistory history1, QualifiedHistory history2)
        {
            if (history1 == null)
                return history2;
            if (history2 == null)
                return history1;
            if (!history1.Qualifier.Equals(history2.Qualifier))
                throw new Exception("Qualifiers don't match!");

            return new QualifiedHistory
            {
                Qualifier = history1.Qualifier,
                Entries = history1.Entries
                    .Union(history2.Entries).Distinct().OrderBy(h => h.DateTimeUtc)
                    .Where(e => e != null)
                    .ToList()
            };
        }
    }
}