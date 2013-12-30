using System.Collections.Generic;
using System.Linq;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// ValueProvider for demonstration purposes only. If you want to write your own implementation, this is a good example of a rather naive way of doing it.
    /// </summary>
    public class DemonstrationValueProvider : ILocalizedValueManager
    {
        private static readonly Locale Locale = new Locale("en");
        public static Dictionary<Qualifier.Unique, QualifiedValue> DefaultValues = new[] {
                CreateValuePair(new Part("Demonstration"), "default", "Demonstration value")
        }.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 

        private static KeyValuePair<Qualifier.Unique, QualifiedValue> CreateValuePair(Part part, string key, string value)
        {
            var qualifier = new Qualifier.Unique(part, Locale, key);
            var qualifiedValue = new QualifiedValue(qualifier, new Value(ContentType.Unspecified, value));
            return new KeyValuePair<Qualifier.Unique, QualifiedValue>(qualifier, qualifiedValue);
        }

        public Dictionary<Qualifier.Unique, QualifiedValue> AllValues = DefaultValues;

        public string GetValue(Qualifier.Unique qualifier, string defaultValue = null)
        {
            return GetQualifiedValue(qualifier, defaultValue).Value.Content;
        }

        public void SetValue(Qualifier.Unique qualifier, string value)
        {
            if (AllValues.ContainsKey(qualifier))
                AllValues[qualifier] = new QualifiedValue(qualifier, new Value(ContentType.Unknown, value));
            else
                AllValues.Add(qualifier, new QualifiedValue(qualifier, new Value(ContentType.Unknown, value)));
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, string defaultValue = null)
        {
            var testQualifier = qualifier;
            while (testQualifier.Part.Parent != null)
            {
                testQualifier = new Qualifier.Unique(testQualifier.Part.Parent, testQualifier.Locale, testQualifier.Key);
                QualifiedValue value;
                if (AllValues.TryGetValue(testQualifier, out value))
                    return value;
            }

            var qualifiedValue = new QualifiedValue(qualifier, new Value(ContentType.Unknown, defaultValue));
            AllValues.Add(qualifier, qualifiedValue);
            return qualifiedValue;
        }

        public void Reload()
        {
            AllValues = DefaultValues;
        }

        public void UpdateCreateValue(QualifiedValue value)
        {
            SetValue(value.Qualifier, value.Value.Content);
        }

        public void CreateValue(Qualifier.Unique qualifier, string value)
        {
            SetValue(qualifier, value);
        }

        public IEnumerable<QualifiedValue> GetAllValuesQualified()
        {
            return AllValues.Values;
        }

        public void DeleteValue(Qualifier.Unique qualifier)
        {
            if (AllValues.ContainsKey(qualifier))
                AllValues.Remove(qualifier);
        }

        public void DeleteLocalizationsFor(Part part, string key)
        {
            var valuesToDelete = AllValues.Where(kvp => Equals(kvp.Key.Part, part) && kvp.Key.Key == key).ToArray();
            foreach (var value in valuesToDelete)
                AllValues.Remove(value.Key);
        }
    }
}
