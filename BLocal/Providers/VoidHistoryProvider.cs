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

        public void AdjustHistory(IEnumerable<QualifiedValue> currentValues, string author)
        {
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
    }
}
