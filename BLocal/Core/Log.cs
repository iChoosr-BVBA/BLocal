using System;
using System.Globalization;

namespace BLocal.Core
{
    public class Log
    {
        public Qualifier.Unique Qualifier { get; private set; }
        public DateTime Date { get; private set; }

        public Log(Qualifier.Unique qualifier, DateTime date)
        {
            Qualifier = qualifier;
            Date = date;
        }

        public override String ToString()
        {
            return String.Format("{0} | {1}", Qualifier, Date.ToString(CultureInfo.InvariantCulture));
        }
    }
}
