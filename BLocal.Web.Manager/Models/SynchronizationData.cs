using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Models
{
    public class SynchronizationData
    {
        public readonly String LeftName;
        public readonly String RightName;
        public readonly QualifiedValue[] LeftValuesNotRight;
        public readonly QualifiedValue[] RightValuesNotLeft;
        public readonly DoubleQualifiedValue[] ValueDifferences;

        public SynchronizationData(string leftName, string rightName, QualifiedValue[] leftValuesNotRight, QualifiedValue[] rightValuesNotLeft, DoubleQualifiedValue[] valueDifferences)
        {
            LeftName = leftName;
            RightName = rightName;
            LeftValuesNotRight = leftValuesNotRight;
            RightValuesNotLeft = rightValuesNotLeft;
            ValueDifferences = valueDifferences;
        }

        public class DoubleQualifiedValue
        {
            public readonly QualifiedValue Left;
            public readonly QualifiedValue Right;

            public DoubleQualifiedValue(QualifiedValue left, QualifiedValue right)
            {
                Left = left;
                Right = right;
            }
        }
    }
}