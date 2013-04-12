using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BLocal.Web.Manager.Configuration
{
    public class ProviderPairsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ProviderPairCollection ProviderPairs
        {
            get { return (ProviderPairCollection)this[""]; }
            set { this[""] = value; }
        }

        public class ProviderPairCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ProviderPairElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ProviderPairElement) element).Name;
            }
        }
    }

    public class ProviderPairElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public String Name
        {
            get { return (String) base["name"]; }
            set { base["name"] = value; }
        }
        [ConfigurationProperty("valueProvider", IsKey = true, IsRequired = true)]
        public ValueProviderElement ValueProvider
        {
            get { return (ValueProviderElement)base["valueProvider"]; }
            set { base["valueProvider"] = value; }
        }
        [ConfigurationProperty("logProvider", IsKey = true, IsRequired = true)]
        public LogProviderElement LogProvider
        {
            get { return (LogProviderElement)base["logProvider"]; }
            set { base["logProvider"] = value; }
        }
    }
    public class ValueProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public String Type
        {
            get { return (String)base["type"]; }
            set { base["type"] = value; }
        }

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ConstructorArgumentCollection ConstructorArguments
        {
            get { return (ConstructorArgumentCollection)this[""]; }
            set { this[""] = value; }
        }

        public class ConstructorArgumentCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ConstructorArgumentElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ConstructorArgumentElement)element).Name;
            }
        } 
    }
    public class LogProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public String Type
        {
            get { return (String)base["type"]; }
            set { base["type"] = value; }
        }
        [ConfigurationProperty("isValueProvider", IsRequired = false)]
        public bool IsValueProvider
        {
            get { return (bool)base["isValueProvider"]; }
            set { base["isValueProvider"] = value; }
        }

        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ConstructorArgumentCollection ConstructorArguments
        {
            get { return (ConstructorArgumentCollection)this[""]; }
            set { this[""] = value; }
        }

        public class ConstructorArgumentCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ConstructorArgumentElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ConstructorArgumentElement)element).Name;
            }
        }
    }

    public class ConstructorArgumentElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public String Name
        {
            get { return (String)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get { return (String)base["value"]; }
            set { base["value"] = value; }
        }
    }

    public class ProviderConfig
    {
        protected static ProviderPairElement[] ValueProviderElements;

        static ProviderConfig()
        {
            var sec = (ProviderPairsConfigurationSection)ConfigurationManager.GetSection("providerPairsConfiguration");
            ValueProviderElements = sec.ProviderPairs.Cast<ProviderPairElement>().ToArray();
        }

        public static IEnumerable<ProviderPairElement> ProviderPairs
        {
            get { return ValueProviderElements; }
        }
    }
}