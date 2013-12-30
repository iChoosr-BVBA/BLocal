using System;
using System.Collections.Generic;

namespace BLocal.Web
{
    public abstract class LocalizedHtmlContainer
    {
        internal readonly bool DebugMode;
        internal RepositoryWrapper Repository { get; set; }
        public String TagName { get; private set; }

        public Dictionary<String, KeyWithDefault> LocalizedAttributes { get; private set; }
        public Dictionary<String, String> OtherAttributes { get; private set; }
        public Dictionary<String, String> Replacements { get; private set; }
        public readonly List<Func<String, String>> Processors = new List<Func<string, string>>(0); 

        internal LocalizedHtmlContainer(RepositoryWrapper repository, String tagName, bool debugMode)
        {
            DebugMode = debugMode;
            Repository = repository;
            TagName = tagName;

            LocalizedAttributes = new Dictionary<String, KeyWithDefault>();
            OtherAttributes = new Dictionary<string, string>();
            Replacements = new Dictionary<String, String>();
        }

        #region basic setters

        protected void Replace(String key, String value)
        {
            Replacements.Add(key, value);
        }

        protected void Process(Func<String, String> processor)
        {
            Processors.Add(processor);
        }

        protected void RemoveAttribute(String attributeName)
        {
            if (LocalizedAttributes.ContainsKey(attributeName))
                LocalizedAttributes.Remove(attributeName);
            if (OtherAttributes.ContainsKey(attributeName))
                OtherAttributes.Remove(attributeName);
        }

        protected void AttrValue(String attributeName, String attributeValue)
        {
            var aName = attributeName.ToLowerInvariant();
            RemoveAttribute(aName);
            OtherAttributes.Add(aName, attributeValue);
        }

        protected void AttrValues(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValues)
        {
            foreach (var attrnv in attributeNamesAndValues)
                AttrValue(attrnv.Key, attrnv.Value);
        }

        protected void AttrsValue(String value, IEnumerable<String> attributes)
        {
            foreach (var attr in attributes)
                AttrValue(attr, value);
        }

        protected void AttrKey(String attributeName, String attributeValueKey, String attributeValueDefault = null)
        {
            var aName = attributeName.ToLowerInvariant();
            RemoveAttribute(aName);
            LocalizedAttributes.Add(aName, new KeyWithDefault(attributeValueKey, attributeValueDefault));
        }

        protected void AttrKeys(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValueKeys)
        {
            foreach (var attrnk in attributeNamesAndValueKeys)
              
                AttrKey(attrnk.Key, attrnk.Value);
        }
        protected void AttrsKey(String key, IEnumerable<String> attrs, String defaultValue = null)
        {
            foreach(var attr in attrs)
                AttrKey(attr, key, defaultValue);
        }

        public struct KeyWithDefault
        {
            public String Key;
            public String Default;

            public KeyWithDefault(String key, String @default)
            {
                Key = key;
                Default = @default;
            }
        }

        #endregion
    }
}