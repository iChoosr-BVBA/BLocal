using System.Collections.Generic;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.DirectEditing
{
    public class IndexModel
    {
        public IEnumerable<LocalizedPart> Parts { get; set; }
        public ProviderGroup Provider { get; set; }
    }
}