using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Models.DirectEditing
{
    public class QualifiedLocalization
    {
        public Qualifier.Unique Qualifier { get; set; }
        public String Value { get; set; }
        public QualifiedHistory History { get; set; }

        public QualifiedLocalization(Qualifier.Unique qualifier, String value, QualifiedHistory history)
        {
            Qualifier = qualifier;
            Value = value;
            History = history;
        }
    }
}