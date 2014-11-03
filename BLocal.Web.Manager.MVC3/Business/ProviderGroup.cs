using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class ProviderGroup
    {
        public readonly ILocalizationHistoryManager HistoryManager;
        public readonly String Name;
        public readonly ILocalizedValueManager ValueManager;
        public readonly ILocalizationLogger Logger;

        public ProviderGroup(String name, ILocalizedValueManager valueManager, ILocalizationHistoryManager historyManager, ILocalizationLogger logger)
        {
            HistoryManager = historyManager;
            Name = name;
            ValueManager = valueManager;
            Logger = logger;
        }
    }
}