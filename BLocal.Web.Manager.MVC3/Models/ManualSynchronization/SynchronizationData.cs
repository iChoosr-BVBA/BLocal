using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Models.ManualSynchronization
{
    public class SynchronizationData
    {
        public readonly String LeftName;
        public readonly String RightName;
        public readonly QualifiedHistoricalValue[] LeftValuesNotRight;
        public readonly QualifiedHistoricalValue[] RightValuesNotLeft;
        public readonly QualifiedHistoricalValuePair[] ValueDifferences;

        public SynchronizationData(String leftName, String rightName, QualifiedHistoricalValue[] leftValuesNotRight, QualifiedHistoricalValue[] rightValuesNotLeft, QualifiedHistoricalValuePair[] valueDifferences)
        {
            LeftName = leftName;
            RightName = rightName;
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