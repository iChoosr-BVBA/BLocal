using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.ManualSynchronization
{
    public class SynchronizationData
    {
        public readonly ProviderGroup Left;
        public readonly ProviderGroup Right;
        public readonly QualifiedHistoricalValue[] LeftValuesNotRight;
        public readonly QualifiedHistoricalValue[] RightValuesNotLeft;
        public readonly QualifiedHistoricalValuePair[] ValueDifferences;

        public SynchronizationData(ProviderGroup left, ProviderGroup right, QualifiedHistoricalValue[] leftValuesNotRight, QualifiedHistoricalValue[] rightValuesNotLeft, QualifiedHistoricalValuePair[] valueDifferences)
        {
            Left = left;
            Right = right;
            LeftValuesNotRight = leftValuesNotRight;
            RightValuesNotLeft = rightValuesNotLeft;
            ValueDifferences = valueDifferences;
        }

        public class QualifiedHistoricalValue
        {
            public QualifiedHistoricalValue(QualifiedValue value, QualifiedHistory history)
            {
                Value = value;
                History = history;
            }

            public QualifiedValue Value { get; set; }
            public QualifiedHistory History { get; set; }
        }

        public class QualifiedHistoricalValuePair
        {
            public readonly QualifiedHistoricalValue Left;
            public readonly QualifiedHistoricalValue Right;

            public QualifiedHistoricalValuePair(QualifiedHistoricalValue left, QualifiedHistoricalValue right)
            {
                Left = left;
                Right = right;
            }
        }
    }
}