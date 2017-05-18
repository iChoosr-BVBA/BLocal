using System;
using System.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Controllers;

namespace BLocal.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type a COMMAND...");
            var command = Console.ReadLine();

            switch (command?.ToLower())
            {
                case "migrate":
                    MigrateCommunitySegmentKeys();
                    break;

                default:
                    Console.WriteLine($"Unknown command '{command}'");
                    break;
            }
            
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static void MigrateCommunitySegmentKeys()
        {
            const string config = "iChoosr - Local";
            var controller = new DirectEditingController();
            var oldPart = "admincommunitysegmentedit";
            var newPart = "admincommunitysegment";

            var locales = new[] {"gb", "nl", "us", "be"};
            var keys = new[]
            {
                "letters-signature-image-caption",
                "letters-signature-text-caption",
                "letters-title",
                "webpage-displayname-caption",
                "webpage-hero-caption",
                "webpage-herotext-caption",
                "webpage-mainlogo-caption",
                "webpage-name-caption",
                "webpage-sme-caption",
                "webpage-sme-url-caption",
                "webpage-title"
            };

            var localization = new ProviderGroupFactory().CreateProviderGroup(config);
            var values = localization.ValueManager.GetAllValuesQualified().ToList();

            foreach (var key in keys)
            {
                foreach (var locale in locales)
                {
                    var qualifier = new Qualifier.Unique(Part.Parse(oldPart), new Locale(locale), key);
                    var content = values.SingleOrDefault(value => value.Qualifier.Equals(qualifier));

                    if (content != null)
                    {
                        var newKey = key.Replace("-caption", "").Replace("-title", "");
                        controller.MoveAndUpdateValue(oldPart, locale, key, newPart, locale, newKey, content.Value, config);
                        Console.WriteLine($"{oldPart}:{key}:{locale} => {newPart}:{newKey}:{locale}");
                    }
                }
            }
        }
    }
}
