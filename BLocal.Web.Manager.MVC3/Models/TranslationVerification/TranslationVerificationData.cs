using System;
using System.Linq;
using BLocal.Core;

namespace BLocal.Web.Manager.Models.TranslationVerification
{
    public class TranslationVerificationData
    {
        public static readonly Locale NoLocale = new Locale("tvd-nolocale-x");

        public readonly ILookup<Qualifier, QualifiedValue> LocaleIndepandantValueLookup;
        public readonly Locale[] LocalesAvailable;
        public readonly String ProviderConfigName;

        public TranslationVerificationData(ILookup<Qualifier, QualifiedValue> localeIndepandantValueLookup, Locale[] localesAvailable, String providerConfigName)
        {
            ProviderConfigName = providerConfigName;
            LocaleIndepandantValueLookup = localeIndepandantValueLookup;
            LocalesAvailable = localesAvailable;
        }
    }
}