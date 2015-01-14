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
    }
}