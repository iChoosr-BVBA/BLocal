using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Linq;

namespace BLocal.Web
{
    public class LocalizedHtmlTag : LocalizedHtmlContainer
    {
        private readonly ViewContext _context;
        public bool IsDisplayed { get; private set; }

        internal LocalizedHtmlTag(RepositoryWrapper repository, String tagName, bool debugMode, ViewContext context)
            : base(repository, tagName, debugMode)
        {
            _context = context;
            IsDisplayed = true;
        }

        #region chaining setters
        /// <summary>
        /// Replace occurences of "key" within the original localized values with the "value" before rendering
        /// </summary>
        /// <param name="key">the string to look for in localized values</param>
        /// <param name="value">the value with which to replace any found keys</param>
        /// <returns>returns itself</returns>
        public new LocalizedHtmlTag Replace(String key, String value)
        {
            base.Replace(key, value);
            return this;
        }
        /// <summary>
        /// Replace occurences of "key" within the original localized values with the "value" before rendering
        /// </summary>
        /// <param name="replacements">A bunch of replacement key/values (like a Dictionary&gt;String, String&lt;</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag Replace(IEnumerable<KeyValuePair<String, String>> replacements)
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
        public LocalizedHtmlTag Process(params Func<String, String>[] processors)
        {
            if (processors == null)
                return this;

            foreach (var processor in processors)
                base.Process(processor);

            return this;
        }

        /// <summary>
        /// Add or override the attribute "attributename" with the value "attributevalue"
        /// </summary>
        /// <param name="attributeName">name of the attribute to add or override</param>
        /// <param name="attributeValue">value for the attribute</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag Attr(String attributeName, String attributeValue)
        {
            AttrValue(attributeName, attributeValue);
            return this;
        }
        /// <summary>
        /// Adds a given value to multiple attributes
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="attrs">the attributes to fill up with the value for this key</param>
        public LocalizedHtmlTag Attr(IEnumerable<String> attrs, String value)
        {
            AttrsValue(value, attrs);
            return this;
        }
        /// <summary>
        /// Add or override multiple attributes
        /// </summary>
        /// <param name="attributeNamesAndValues">name/value collection (Dictionary works) for attributes with their corresponding values</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag Attr(IEnumerable<KeyValuePair<string, string>> attributeNamesAndValues)
        {
            AttrValues(attributeNamesAndValues);
            return this;
        }

        /// <summary>
        /// Adds (does not overwrite) classes to the tag
        /// </summary>
        /// <param name="classNames">the classname(s) to add</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag Class(params String[] classNames)
        {
            const String classAttr = "class";
            foreach (var className in classNames) {
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
        public LocalizedHtmlTag ClassIf(bool condition, params String[] classNames)
        {
            return condition ? Class(classNames) : this;
        }

        /// <summary>
        /// Sets the ID for the tag
        /// </summary>
        /// <param name="id">the id to set</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag Id(String id)
        {
            return Attr("id", id);
        }
        /// <summary>
        /// Sets the ID for the tag if condition evaluates as true
        /// </summary>
        /// <param name="condition">if condition evaluates as true, sets id, otherwise discards.</param>
        /// <param name="id">the id to set</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag IdIf(bool condition, String id)
        {
            return condition ? Id(id) : this;
        }

        /// <summary>
        /// Add or overide the attribute "attributename" with the localized value for "attributevaluekey"
        /// </summary>
        /// <param name="attributeName">name of the attribute to add or override</param>
        /// <param name="attributeValueKey">key to be used to look up the value for the attribute</param>
        /// <param name="defaultAttributeValue">Default value for the attribute</param>
        /// <returns>returns itself</returns>
        public new LocalizedHtmlTag AttrKey(String attributeName, String attributeValueKey, String defaultAttributeValue = null)
        {
            base.AttrKey(attributeName, attributeValueKey, defaultAttributeValue);
            return this;
        }
        /// <summary>
        /// Adds the localized value for one key to multiple attributes
        /// </summary>
        /// <param name="key">the key to look up</param>
        /// <param name="attrs">the attributes to fill up with the value for this key</param>
        public LocalizedHtmlTag AttrKey(IEnumerable<String> attrs, String key)
        {
            AttrsKey(key, attrs);
            return this;
        }
        /// <summary>
        /// Add or override multiple localized attributes
        /// </summary>
        /// <param name="attributeNamesAndValueKeys">name/valuekey collection (Dictionary works) for attributes with the keys for their corresponding localized values</param>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag AttrKey(IEnumerable<KeyValuePair<String, String>> attributeNamesAndValueKeys)
        {
            AttrKeys(attributeNamesAndValueKeys);
            return this;
        }

        /// <summary>
        /// Only wraps node if condition is true. When multiple conditions are specified, all of them must be true in order for the component to wrap.
        /// When any of the conditions evaluate to false, the inner content is still shown, it is only the localized div that will not appear in the DOM
        /// Node will display anyway if debug mode is active (that's the whole point)
        /// </summary>
        /// <returns>returns itself</returns>
        public LocalizedHtmlTag WrapIf(bool condition)
        {
            IsDisplayed &= condition;
            return this;
        }
        #endregion

        #region rendering the HTML
        /// <summary>
        /// Returns an IDisposable LocalWrapper that will render the open tag upon opening, and a close tag upon closing
        /// </summary>
        /// <returns>returns a LocalWrapper containing the rendered tag</returns>
        public LocalWrapper Open()
        {
            if (!(IsDisplayed || DebugMode))
                return new LocalWrapper(null, _context);

            var builder = new TagBuilder(TagName);

            foreach (var attribute in LocalizedAttributes) {
                var value = Repository.GetQualified(attribute.Value.Key, attribute.Value.Default);
                builder.MergeAttribute(attribute.Key, Processors.Aggregate(
                    value.Value.DecodeWithReplacements(Replacements),
                    (val, processor) => processor(val)
                ));

                if (!DebugMode) continue;
                builder.MergeAttribute("data-loc-debug", "true");
                var key = "data-loc-attribute-" + attribute.Key + "-";
                builder.MergeAttribute(key + "part", value.Qualifier.Part.ToString());
                builder.MergeAttribute(key + "content", value.Value.Content);
            }

            if (DebugMode) {

                if (LocalizedAttributes.Any()) {
                    var localizations = String.Join(((char)31).ToString(CultureInfo.InvariantCulture), LocalizedAttributes.Select(localization => localization.Key + (char)30 + localization.Value));
                    builder.MergeAttribute("data-loc-localizations", localizations);
                }

                if (Replacements.Any()) {
                    var replacements = String.Join(((char)31).ToString(CultureInfo.InvariantCulture), Replacements.Select(replacement => replacement.Key + (char)30 + replacement.Value));
                    builder.MergeAttribute("data-loc-replacements", replacements);
                }
            }

            foreach (var attribute in OtherAttributes)
                builder.MergeAttribute(attribute.Key, attribute.Value);

            return new LocalWrapper(builder, _context);
        }
        #endregion
    }
}