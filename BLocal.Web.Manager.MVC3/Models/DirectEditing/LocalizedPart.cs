using System.Collections.Generic;
using BLocal.Core;
using BLocal.Web.Manager.Models.Home;

namespace BLocal.Web.Manager.Models.DirectEditing
{
    public class LocalizedPart
    {
        public List<LocalizedPart> Subparts { get; set; }
        public List<QualifiedLocalization> Localizations { get; set; }
        public Part Part { get; set; }

        public LocalizedPart(Part part, List<QualifiedLocalization> localizations)
        {
            Part = part;
            Localizations = localizations;
            Subparts = new List<LocalizedPart>();
        }
    }
}