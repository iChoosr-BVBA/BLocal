using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Business
{
    public class ProviderGroup
    {
        public readonly ILocalizationHistoryManager HistoryManager;
        public readonly String Name;
        public readonly String Color;
        public readonly ILocalizedValueManager ValueManager;
        public readonly ILocalizationLogger Logger;

        public ProviderGroup(String name, String color, ILocalizedValueManager valueManager, ILocalizationHistoryManager historyManager, ILocalizationLogger logger)
        {
            HistoryManager = historyManager;
            Name = name;
            Color = String.IsNullOrWhiteSpace(color) ? "#2b2" : color;
            ValueManager = valueManager;
            Logger = logger;
        }
    }
}