using System;
using System.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Business;

namespace BLocal.Web.Manager.Models.TranslationVerification
{
    public class TranslationVerificationData
    {
        public static readonly Locale NoLocale = new Locale("tvd-nolocale-x");

        public readonly ILookup<Qualifier, QualifiedValue> LocaleIndepandantValueLookup;
        public readonly Locale[] LocalesAvailable;
        public readonly ProviderGroup Provider;

        public TranslationVerificationData(ILookup<Qualifier, QualifiedValue> localeIndepandantValueLookup, Locale[] localesAvailable, ProviderGroup provider)
        {
            Provider = provider;
            LocaleIndepandantValueLookup = localeIndepandantValueLookup;
            LocalesAvailable = localesAvailable;
        }
    }
}