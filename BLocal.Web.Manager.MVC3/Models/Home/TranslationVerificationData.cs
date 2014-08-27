using System.Linq;
using BLocal.Core;

namespace BLocal.Web.Manager.Models
{
    public class TranslationVerificationData
    {
        public static readonly Locale NoLocale = new Locale("tvd-nolocale-x");

        public readonly ILookup<Qualifier, QualifiedValue> LocaleIndepandantValueLookup;
        public readonly Locale[] LocalesAvailable;

        public TranslationVerificationData(ILookup<Qualifier, QualifiedValue> localeIndepandantValueLookup, Locale[] localesAvailable)
        {
            LocaleIndepandantValueLookup = localeIndepandantValueLookup;
            LocalesAvailable = localesAvailable;
        }
    }
}