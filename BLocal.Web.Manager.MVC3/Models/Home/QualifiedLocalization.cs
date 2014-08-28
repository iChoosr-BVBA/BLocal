using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Models
{
    public class QualifiedLocalization
    {
        public Qualifier.Unique Qualifier { get; set; }
        public String Value { get; set; }
        public DateTime LastAcces { get; set; }

        public QualifiedLocalization(Qualifier.Unique qualifier, String value, DateTime lastAccess)
        {
            Qualifier = qualifier;
            Value = value;
            LastAcces = lastAccess;
        }
    }
}