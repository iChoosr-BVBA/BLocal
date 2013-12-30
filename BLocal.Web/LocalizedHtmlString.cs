using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    public class LocalizedHtmlString : LocalizedHtmlContainer, IHtmlString
    {
        public KeyWithDefault LocalizedHtmlKey { get; private set; }
        public String OtherHtml { get; private set; }
        private readonly List<LocalizedHtmlString> _children = new List<LocalizedHtmlString>();
        public bool IsDisplayed { get; private set; }

        internal LocalizedHtmlString(RepositoryWrapper repository, String tagName, bool debugMode)
            : base(repository, tagName, debugMode)
        {
            IsDisplayed = true;
        }

        #region chaining setters
        /// <summary>
        /// Replace occurences of "key" within the original localized values with the ToString output of "value" before rendering.
        /// </summary>
        /// <param name="key">the string to look for in localized values</param>
        /// <param name="value">the value with which to replace any found keys</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Replace(String key, Object value)
        {
            base.Replace(key, value.ToString());
            foreach (var child in _children)
                child.Replace(key, value.ToString());
            return this;
        }
        /// <summary>
        /// Replace occurences of "key" within the original localized values with the "value" before rendering
        /// </summary>
        /// <param name="replacements">A bunch of replacement key/values (like a Dictionary&gt;String, String&lt;</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Replace(IEnumerable<KeyValuePair<String, String>> replacements)
        {
            if (replacements == null)
                return this;

            foreach (var kvp in replacements)
                Replace(kvp.Key, kvp.Value);

            return this;
        }

        /// <summary>
        /// Before this tag is rendered, all of its localized values will pass through the processors. This can be used, for instance, strip HTML tags from the localized values.
        /// </summary>
        /// <param name="processors">Function that changes a raw localized value into a processed value and returns that processed value</param>
        public LocalizedHtmlString Process(params Func<String, String>[] processors)
        {
            if (processors == null)
                return this;

            foreach (var processor in processors)
                base.Process(processor);

            foreach(var child in _children)
                child.Process(processors);

            return this;
        }

        /// <summary>
        /// Add or override the attribute "attributename" with the value "attributevalue"
        /// </summary>
        /// <param name="attributeName">name of the attribute to add or override</param>
        /// <param name="attributeValue">value for the attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Attr(String attributeName, String attributeValue)
        {
            AttrValue(attributeName, attributeValue);
            return this;
        }
        /// <summary>
        /// Adds a given value to multiple attributes
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="attrs">the attributes to fill up with the value for this key</param>
        public LocalizedHtmlString Attr(IEnumerable<String> attrs, String value)
        {
            AttrsValue(value, attrs);
            return this;
        }
        /// <summary>
        /// Add or override multiple attributes
        /// </summary>
        /// <param name="attributeNamesAndValues">name/value collection (Dictionary works) for attributes with their corresponding values</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Attr(IEnumerable<KeyValuePair<string, string>> attributeNamesAndValues)
        {
            AttrValues(attributeNamesAndValues);
            return this;
        }
        /// <summary>
        /// Add or override the attribute "attributename" with the value "attributevalue" if "condition" is true
        /// </summary>
        /// <param name="condition">if the condition is false, this method will do nothing</param>
        /// <param name="attributeName">name of the attribute to add or override</param>
        /// <param name="attributeValue">value for the attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString AttrIf(bool condition, String attributeName, String attributeValue)
        {
            return condition ? Attr(attributeName, attributeValue) : this;
        }

        /// <summary>
        /// Add or override the attribute data-"attributename" with the value "attributevalue"
        /// </summary>
        /// <param name="attributeName">name of the data attribute to add or override</param>
        /// <param name="attributeValue">value for the attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Data(String attributeName, Object attributeValue)
        {
            AttrValue("data-" + attributeName, attributeValue.ToString());
            return this;
        }
        /// <summary>
        /// Add or override the attribute data-"attributename" with the value "attributevalue"
        /// </summary>
        /// <param name="attributeName">name of the data attribute to add or override</param>
        /// <param name="attributeValueKey">key to look for in the localized values to put as the value for the attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString DataKey(String attributeName, String attributeValueKey)
        {
            AttrKey("data-" + attributeName, attributeValueKey);
            return this;
        }
        /// <summary>
        /// Adds (does not overwrite) classes to the tag
        /// </summary>
        /// <param name="classNames">the classname(s) to add</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Class(params String[] classNames)
        {
            const String classAttr = "class";
            foreach (var className in classNames)
            {
                if (LocalizedAttributes.ContainsKey(classAttr))
                    LocalizedAttributes.Remove(classAttr);

                if (OtherAttributes.ContainsKey(classAttr))
                    OtherAttributes[classAttr] = OtherAttributes[classAttr] + " " + className;
                else
                    OtherAttributes.Add(classAttr, className);
            }
            return this;
        }
        /// <summary>
        /// Adds (does not overwrite) classes to the tag if condition evaluates as true
        /// </summary>
        /// <param name="condition">if condition evaluates as true, adds class, otherwise discards.</param>
        /// <param name="classNames">the classname(s) to add</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString ClassIf(bool condition, params String[] classNames)
        {
            if(condition)
                Class(classNames);
            return this;
        }

        /// <summary>
        /// Sets the ID for the tag
        /// </summary>
        /// <param name="id">the id to set</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Id(String id)
        {
            return Attr("id", id);
        }
        /// <summary>
        /// Sets the ID for the tag if condition evaluates as true
        /// </summary>
        /// <param name="condition">if condition evaluates as true, sets id, otherwise discards.</param>
        /// <param name="id">the id to set</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString IdIf(bool condition, String id)
        {
            return condition ? Id(id) : this;
        }
        /// <summary>
        /// sets the "name" attribute of the HTML element
        /// </summary>
        /// <param name="name">the value for the id</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Name(String name)
        {
            Attr("name", name);
            return this;
        }
        /// <summary>
        /// sets the "value" attribute of the HTML element
        /// </summary>
        /// <param name="value">the value for the "value" attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Val(String value)
        {
            Attr("value", value);
            return this;
        }

        /// <summary>
        /// Add or overide the attribute "attributename" with the localized value for "attributevaluekey"
        /// </summary>
        /// <param name="attributeName">name of the attribute to add or override</param>
        /// <param name="attributeValueKey">key to be used to look up the value for the attribute</param>
        /// <param name="defaultValue">Default value to create if no value is found</param>
        /// <returns>returns itself</returns>
        public new LocalizedHtmlString AttrKey(String attributeName, String attributeValueKey, String defaultValue = null)
        {
            base.AttrKey(attributeName, attributeValueKey, defaultValue);
            return this;
        }
        /// <summary>
        /// Adds the localized value for one key to multiple attributes
        /// </summary>
        /// <param name="attrs">the attributes to fill up with the value for this key</param>
        /// <param name="key">the key to look up</param>
        /// <param name="defaultValue">Default value to create if no value is found</param>
        public LocalizedHtmlString AttrKey(IEnumerable<String> attrs, String key, String defaultValue = null)
        {
            AttrsKey(key, attrs, defaultValue);
            return this;
        }
        /// <summary>
        /// Add or override multiple localized attributes
        /// </summary>
        /// <param name="attributeNamesAndValueKeys">name/valuekey collection (Dictionary works) for attributes with the keys for their corresponding localized values</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString AttrKey(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValueKeys, String defaultValue = null)
        {
            AttrKeys(attributeNamesAndValueKeys);
            return this;
        }

        /// <summary>
        /// Sets the inner HTML for this element. If a HtmlKey is set, appends this after the localized value for that key.
        /// </summary>
        /// <param name="html">the text to display as inner HTML</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString Html(String html)
        {
            OtherHtml = html;
            return this;
        }

        /// <summary>
        /// Sets the key that will be used to look up the value for the inner HTML of the node.
        /// </summary>
        /// <param name="htmlKey">key to use for lookup</param>
        /// <param name="defaultValue">Default value to create if no value is found for the key</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString HtmlKey(String htmlKey, string defaultValue = null)
        {
            LocalizedHtmlKey = new KeyWithDefault(htmlKey, defaultValue);
            return this;
        }

        /// <summary>
        /// Registers itself to the parent, inheriting all of its replacement parameters
        /// </summary>
        /// <param name="parent">parent node whose replacement parameters to take over</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString WithParent(LocalizedHtmlString parent)
        {
            parent.Register(this);
            return this;
        }
        /// <summary>
        /// Only displays node if condition is true. When multiple conditions are specified, all of them must be true in order for the component to display.
        /// Node will display anyway if debug mode is active (that's the whole point)
        /// </summary>
        /// <returns>returns itself</returns>
        public LocalizedHtmlString DisplayIf(bool condition)
        {
            IsDisplayed &= condition;
            return this;
        }

        private void Register(LocalizedHtmlString child)
        {
            _children.Add(child);
            foreach (var replacement in Replacements)
                child.Replace(replacement.Key, replacement.Value);
            child.Process(Processors.ToArray());
        }
        #endregion

        #region rendering the HTML
        public string ToHtmlString()
        {
            if (!(IsDisplayed || DebugMode))
                return String.Empty;

            var builder = new TagBuilder(TagName);

            var innerHtml = String.Empty;
            QualifiedValue innerHtmlValue = null;

            if (!String.IsNullOrEmpty(LocalizedHtmlKey.Key)) {
                innerHtmlValue = Repository.GetQualified(LocalizedHtmlKey.Key, LocalizedHtmlKey.Default);
                var decodedValue = Processors.Aggregate(
                    innerHtmlValue.Value.DecodeWithReplacements(Replacements),
                    (value, processor) => processor(value)
                );
                builder.InnerHtml = decodedValue;
            }

            foreach (var attribute in LocalizedAttributes) {
                var value = Repository.GetQualified(attribute.Value.Key, attribute.Value.Default);
                builder.MergeAttribute(attribute.Key, Processors.Aggregate(
                    value.Value.DecodeWithReplacements(Replacements),
                    (val, processor) => processor(val)
                ));

                if (!DebugMode) continue;
                var key = "data-loc-attribute-" + attribute.Key + "-";
                builder.MergeAttribute(key + "part", value.Qualifier.Part.ToString());
                builder.MergeAttribute(key + "content", value.Value.Content);
            }

            if (DebugMode) {
                builder.MergeAttribute("data-loc-debug", "true");

                if (innerHtmlValue != null) {
                    builder.MergeAttribute("data-loc-inner-part", innerHtmlValue.Qualifier.Part.ToString());
                    builder.MergeAttribute("data-loc-inner-key", innerHtmlValue.Qualifier.Key);
                    builder.MergeAttribute("data-loc-inner-value", innerHtmlValue.Value.Content);
                }

                if (LocalizedAttributes.Any()) {
                    var localizations = String.Join(((char)31).ToString(CultureInfo.InvariantCulture), LocalizedAttributes.Select(localization => localization.Key + (char)30 + localization.Value));
                    builder.MergeAttribute("data-loc-localizations", localizations);
                }

                if (Replacements.Any()) {
                    var replacements = String.Join(((char)31).ToString(CultureInfo.InvariantCulture), Replacements.Select(replacement => replacement.Key + (char)30 + replacement.Value));
                    builder.MergeAttribute("data-loc-replacements", replacements);
                }
            }

            if (!String.IsNullOrEmpty(OtherHtml))
                innerHtml += OtherHtml;
            if (!String.IsNullOrEmpty(innerHtml))
                builder.InnerHtml = innerHtml;

            foreach (var attribute in OtherAttributes)
                builder.MergeAttribute(attribute.Key, attribute.Value);

            return builder.ToString();
        }
        #endregion
    }
}