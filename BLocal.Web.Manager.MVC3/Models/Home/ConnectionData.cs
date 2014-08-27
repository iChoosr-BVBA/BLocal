using System;

namespace BLocal.Web.Manager.Models
{
    public class ConnectionData
    {
        public String ConnectionString { get; set; }
        public String Schema { get; set; }
        public String PartTable { get; set; }
        public String LocaleTable { get; set; }
        public String LogTable { get; set; }
        public String ValueTable { get; set; }
    }
}