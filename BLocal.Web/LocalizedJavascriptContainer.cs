using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    public class LocalizedJavascriptContainer : IHtmlString
    {
        private readonly LocalizationHelper _localization;
        private readonly RepositoryWrapper _repository;
        private readonly List<Tuple<QualifiedValue, List<KeyValuePair<string, string>>>> _qualifiedValuesWithReplacements; 

        public LocalizedJavascriptContainer(LocalizationHelper localization)
        {
            _localization = localization;
            _repository = localization.Repository;
            _qualifiedValuesWithReplacements = new List<Tuple<QualifiedValue, List<KeyValuePair<string, string>>>>();
        }

        /// <summary>
        /// Make localization for key available in javascript (uses default Part & Locale)
        /// </summary>
        /// <param name="key">The key to look up in the localization</param>
        public LocalizedJavascriptContainer Add(String key)
        {
            var qualifiedValue = _repository.GetQualified(key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    new List<KeyValuePair<string, string>>()
                )
            );
            return this;
        }

        /// <summary>
        /// Make localization for keys available in javascript (uses default Part & Locale)
        /// </summary>
        /// <param name="keys">The keys to look up in the localization</param>
        public LocalizedJavascriptContainer AddMultiple(params String[] keys)
        {
            foreach(var key in keys)
                Add(key);

            return this;
        }

        /// <summary>
        /// Make localization for key available in javascript (uses default Part & Locale). Add a single replacement.
        /// </summary>
        /// <param name="key">The key to look up in the localization</param>
        /// <param name="replacementKey">The text to look for and  replace</param>
        /// <param name="replacementValue">The replacement value </param>
        public LocalizedJavascriptContainer Add(String key, String replacementKey, String replacementValue)
        {
            var qualifiedValue = _repository.GetQualified(key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(replacementKey, replacementValue) }
                )
            );
            return this;
        }

        /// <summary>
        /// Make localization for key available in javascript (uses default Part & Locale). Add multiple replacements.
        /// </summary>
        /// <param name="key">The key to look up in the localization</param>
        /// <param name="replacements">Replacements to make in the localized value</param>
        public LocalizedJavascriptContainer Add(String key, List<KeyValuePair<string, string>> replacements)
        {
            var qualifiedValue = _repository.GetQualified(key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    replacements.ToList()
                )
            );
            return this;
        }

        /// <summary>
        /// Make localization for qualified lookup in javascript.
        /// </summary>
        /// <param name="part">The part for lookup in the localization</param>
        /// <param name="locale">The locale for lookup in the localization</param>
        /// <param name="key">The key to look up in the localization</param>
        public LocalizedJavascriptContainer Add(Part part, Locale locale, String key)
        {
            var qualifiedValue = _repository.GetQualified(part, locale, key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    new List<KeyValuePair<string, string>>()
                )
            );
            return this;
        }

        /// <summary>
        /// Make localization for qualified lookup in javascript. Add a single replacement.
        /// </summary>
        /// <param name="part">The part for lookup in the localization</param>
        /// <param name="locale">The locale for lookup in the localization</param>
        /// <param name="key">The key to look up in the localization</param>
        /// <param name="replacementKey">The text to look for and  replace</param>
        /// <param name="replacementValue">The replacement value </param>
        public LocalizedJavascriptContainer Add(Part part, Locale locale, String key, String replacementKey, String replacementValue)
        {
            var qualifiedValue = _repository.GetQualified(part, locale, key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(replacementKey, replacementValue) }
                )
            );
            return this;
        }

        /// <summary>
        /// Make localization for qualified lookup in javascript. Add multiple replacements
        /// </summary>
        /// <param name="part">The part for lookup in the localization</param>
        /// <param name="locale">The locale for lookup in the localization</param>
        /// <param name="key">The key to look up in the localization</param>
        /// <param name="replacements">Replacements to make in the localized value</param>
        public LocalizedJavascriptContainer Add(Part part, Locale locale, String key, List<KeyValuePair<string, string>> replacements)
        {
            var qualifiedValue = _repository.GetQualified(part, locale, key);
            _localization.AddIndirectValue(qualifiedValue);
            _qualifiedValuesWithReplacements.Add(
                Tuple.Create(
                    qualifiedValue,
                    replacements.ToList()
                )
            );
            return this;
        }

        string IHtmlString.ToHtmlString()
        {
            var tag = new TagBuilder("script");
            tag.MergeAttribute("type", "text/javascript");
            tag.InnerHtml = "localization.load(" + JsonAll() + ");";
            return tag.ToString();
        }

        private String JsonAll()
        {
            return "{" + String.Join(",", _qualifiedValuesWithReplacements.Select(qv =>
                JsonQv(qv.Item1, qv.Item2)
            )) + "}";
        }

        private static String JsonQv(QualifiedValue qv, IEnumerable<KeyValuePair<string, string>> replacements)
        {
            return String.Format("'{2}':{{part:'{0}', locale:'{1}', key: '{2}', value: '{3}', origvalue: '{4}', replacements: [{5}] }}",
                qv.Qualifier.Part.ToString().Replace("'", @"\'").Replace(@"\", @"\\"),
                qv.Qualifier.Locale.ToString().Replace("'", @"\'").Replace(@"\", @"\\"),
                qv.Qualifier.Key.Replace("'", @"\'").Replace(@"\", @"\\"),
                qv.Value.DecodedContent.Replace("'", @"\'").Replace(@"\", @"\\"),
                qv.Value.Content.Replace("'", @"\'").Replace(@"\", @"\\"),
                JsonRepl(replacements)
            );
        }

        private static String JsonRepl(IEnumerable<KeyValuePair<string, string>> replacements)
        {
            return String.Join(",", replacements.Select(r =>
                String.Format("{{key: '{0}', value: '{1}'}}",
                    r.Key.Replace("'", @"\'").Replace(@"\", @"\\"),
                    r.Value.Replace("'", @"\'").Replace(@"\", @"\\")
                )
            ));
        }
    }
}
