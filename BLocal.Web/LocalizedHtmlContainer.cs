using System;
using System.Collections.Generic;

namespace BLocal.Web
{
    public abstract class LocalizedHtmlContainer
    {
        internal readonly bool DebugMode;
        internal RepositoryWrapper Repository { get; set; }
        public String TagName { get; private set; }

        public Dictionary<String, String> LocalizedAttributes { get; private set; }
        public Dictionary<String, String> OtherAttributes { get; private set; }
        public Dictionary<String, String> Replacements { get; private set; }
        public readonly List<Func<String, String>> Processors = new List<Func<string, string>>(0); 

        internal LocalizedHtmlContainer(RepositoryWrapper repository, String tagName, bool debugMode)
        {
            DebugMode = debugMode;
            Repository = repository;
            TagName = tagName;

            LocalizedAttributes = new Dictionary<String, String>();
            OtherAttributes = new Dictionary<string, string>();
            Replacements = new Dictionary<String, String>();
        }

        #region basic setters

        internal void Replace(String key, String value)
        {
            Replacements.Add(key, value);
        }

        internal void Process(Func<String, String> processor)
        {
            Processors.Add(processor);
        }

        internal void RemoveAttribute(String attributeName)
        {
            if (LocalizedAttributes.ContainsKey(attributeName))
                LocalizedAttributes.Remove(attributeName);
            if (OtherAttributes.ContainsKey(attributeName))
                OtherAttributes.Remove(attributeName);
        }

        internal void AttrValue(String attributeName, String attributeValue)
        {
            var aName = attributeName.ToLowerInvariant();
            RemoveAttribute(aName);
            OtherAttributes.Add(aName, attributeValue);
        }

        internal void AttrValues(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValues)
        {
            foreach (var attrnv in attributeNamesAndValues)
                AttrValue(attrnv.Key, attrnv.Value);
        }

        internal void AttrsValue(String value, IEnumerable<String> attributes)
        {
            foreach (var attr in attributes)
                AttrValue(attr, value);
        }

        internal void AttrKey(String attributeName, String attributeValueKey)
        {
            var aName = attributeName.ToLowerInvariant();
            RemoveAttribute(aName);
            LocalizedAttributes.Add(aName, attributeValueKey);
        }

        internal void AttrKeys(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValueKeys)
        {
            foreach (var attrnk in attributeNamesAndValueKeys)
              
                AttrKey(attrnk.Key, attrnk.Value);
        }
        internal void AttrsKey(String key, IEnumerable<String> attrs)
        {
            foreach(var attr in attrs)
                AttrKey(attr, key);
        }

        #endregion
    }
}