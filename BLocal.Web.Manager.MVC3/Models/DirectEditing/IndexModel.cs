using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BLocal.Web.Manager.Models.DirectEditing
{
    public class IndexModel
    {
        public String ProviderConfigName { get; set; }
        public IEnumerable<LocalizedPart> Parts { get; set; } 
    }
}