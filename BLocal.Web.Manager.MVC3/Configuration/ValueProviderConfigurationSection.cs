using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BLocal.Web.Manager.Configuration
{
    public class ProviderGroupsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ProviderGroupCollection ProviderGroups
        {
            get { return (ProviderGroupCollection)this[""]; }
            set { this[""] = value; }
        }

        public class ProviderGroupCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new ProviderGroupElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ProviderGroupElement)element).Name;
            }
        }
    }

    public class ProviderGroupElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public String Name
        {
            get { return (String) base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("color", IsKey = false, IsRequired = false)]
        public String Color
        {
            get { return (String)base["color"]; }
            set { base["color"] = value; }
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

        [ConfigurationProperty("historyProvider", IsKey = true, IsRequired = true)]
        public HistoryProviderElement HistoryProvider
        {
            get { return (HistoryProviderElement)base["historyProvider"]; }
            set { base["historyProvider"] = value; }
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
    }

    public class HistoryProviderElement : ConfigurationElement
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
        protected static ProviderGroupElement[] ValueProviderElements;

        static ProviderConfig()
        {
            var sec = (ProviderGroupsConfigurationSection)ConfigurationManager.GetSection("providerGroupsConfiguration");
            ValueProviderElements = sec.ProviderGroups.Cast<ProviderGroupElement>().ToArray();
        }

        public static IEnumerable<ProviderGroupElement> ProviderGroups
        {
            get { return ValueProviderElements; }
        }
    }
}