using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class ProviderPair
    {
        public readonly String Name;
        public readonly ILocalizedValueManager ValueManager;
        public readonly ILocalizationLogger Logger;

        public ProviderPair(String name, ILocalizedValueManager valueManager, ILocalizationLogger logger)
        {
            Name = name;
            ValueManager = valueManager;
            Logger = logger;
        }
    }
}