using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Models
{
    public class QualifiedLocalization
    {
        public Qualifier.Unique Qualifier { get; set; }
        public Value Value { get; set; }
        public DateTime LastAcces { get; set; }

        public QualifiedLocalization(Qualifier.Unique qualifier, Value value, DateTime lastAccess)
        {
            Qualifier = qualifier;
            Value = value;
            LastAcces = lastAccess;
        }
    }
}