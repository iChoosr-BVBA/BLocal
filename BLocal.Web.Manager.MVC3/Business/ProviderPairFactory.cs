using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Configuration;

namespace BLocal.Web.Manager.Business
{
    public class ProviderPairFactory
    {
        public ProviderPair CreateProviderPair(String providerConfigName)
        {
            var providerConfig = ProviderConfig.ProviderPairs.Single(vp => vp.Name == providerConfigName);

            var valueProviderType = Type.GetType(providerConfig.ValueProvider.Type);
            if (valueProviderType == null)
                throw new Exception("Cannot find type \"" + providerConfig.ValueProvider.Type + "\"");
            if (!typeof(ILocalizedValueManager).IsAssignableFrom(valueProviderType))
                throw new Exception("Type \"" + valueProviderType + "\" does not implement " + typeof(ILocalizedValueManager).Name + "!");

            var valueProvider = ConstructProvider(valueProviderType, providerConfig.ValueProvider.ConstructorArguments.Cast<ConstructorArgumentElement>().ToArray());
            if (valueProvider == null)
                throw new Exception("Could not initialize value provider, incorrect constructor arguments!");

            if (providerConfig.LogProvider.IsValueProvider)
                return new ProviderPair(providerConfigName, valueProvider as ILocalizedValueManager, valueProvider as ILocalizationLogger);

            var logProviderType = Type.GetType(providerConfig.LogProvider.Type);
            if (logProviderType == null)
                throw new Exception("Cannot find type \"" + providerConfig.LogProvider.Type + "\"");
            if (!typeof(ILocalizationLogger).IsAssignableFrom(logProviderType))
                throw new Exception("Type \"" + logProviderType + "\" does not implement " + typeof(ILocalizationLogger).Name + "!");

            var logProvider = ConstructProvider(logProviderType, providerConfig.LogProvider.ConstructorArguments.Cast<ConstructorArgumentElement>().ToArray());
            if (logProvider == null)
                throw new Exception("Could not initialize log provider, incorrect constructor arguments!");

            var pair = new ProviderPair(providerConfigName, valueProvider as ILocalizedValueManager, logProvider as ILocalizationLogger);
            pair.ValueManager.Reload();
            return pair;
        }

        private static Object ConstructProvider(Type providerType, ICollection<ConstructorArgumentElement> arguments)
        {
            foreach (var constructor in providerType.GetConstructors())
            {
                var parameters = constructor.GetParameters().ToArray();

                // not all arguments can be specified
                if (parameters.Select(param => param.Name).Intersect(arguments.Select(a => a.Name)).Count() != arguments.Count)
                    continue;

                // not enough arguments to call constructor
                if (parameters.Any(param => !(param.IsOptional || param.DefaultValue != null || arguments.Any(arg => arg.Name == param.Name))))
                    continue;

                // get correct arguments
                var invocationArguments = parameters
                    .OrderBy(param => param.Position)
                    .Select(p => new { Parameter = p, Argument = arguments.SingleOrDefault(a => a.Name == p.Name) })
                    .Select(pair => pair.Argument == null ? pair.Parameter.DefaultValue : pair.Argument.Value)
                    .ToArray();

                // invoke constructor
                return constructor.Invoke(invocationArguments);
            }
            return null;
        }
    }
}