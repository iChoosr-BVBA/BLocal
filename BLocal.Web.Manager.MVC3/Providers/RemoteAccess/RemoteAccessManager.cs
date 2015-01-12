using System;
using System.Collections.Generic;
using System.Linq;
using BLocal.Core;
using BLocal.Web.Manager.Providers.RemoteAccess.Communication;

namespace BLocal.Web.Manager.Providers.RemoteAccess
{
    public class RemoteAccessManager : ILocalizedValueManager, ILocalizationHistoryManager
    {
        private readonly string _targetPassword;
        private readonly RemoteAccessConnector _connector;
        private Dictionary<Part, KeyLocaleValueContainer> _localizedValues = new Dictionary<Part, KeyLocaleValueContainer>();
        private Dictionary<Qualifier.Unique, QualifiedHistory> _history = new Dictionary<Qualifier.Unique, QualifiedHistory>();

        public RemoteAccessManager(String targetUrl, String targetPassword, String targetProviderGroup)
        {
            _targetPassword = targetPassword;
            _connector = new RemoteAccessConnector(targetUrl, targetProviderGroup);
        }

        public String GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            return GetQualifiedValue(qualifier, defaultValue).Value;
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            var part = qualifier.Part;
            do
            {
                if (_localizedValues.ContainsKey(part))
                {
                    var content = _localizedValues[part].GetLocaleValueContainerFor(qualifier.Key).GetValueFor(qualifier.Locale.Name);
                    return new QualifiedValue(new Qualifier.Unique(part, qualifier.Locale, qualifier.Key), content);
                }

                part = part.Parent;
            } while (part != null);

            throw new KeyNotFoundException();
        }

        public void Persist()
        {
            _connector.Persist();
        }

        public void Reload()
        {
            _connector.Authenticate(_targetPassword);
            Reload(_connector.GetAllQualifiedValues());
            Reload(_connector.ProvideHistory());
        }

        public void ProgressHistory(QualifiedValue value, String author)
        {
            _connector.ProgressHistory(value, author);

            if(!_history.ContainsKey(value.Qualifier))
                _history.Add(value.Qualifier, new QualifiedHistory { Qualifier = value.Qualifier });

            _history[value.Qualifier].Entries.Add(new QualifiedHistoryEntry {
                Author = author,
                Content = value.Value,
                ContentHash = new ContentHasher().CalculateHash(value.Value),
                DateTimeUtc = DateTime.UtcNow
            });
        }

        public QualifiedHistory GetHistory(Qualifier.Unique qualifier)
        {
            return _history.ContainsKey(qualifier) ? _history[qualifier] : null;
        }

        public void OverrideHistory(QualifiedHistory qualifiedHistory)
        {
            _connector.OverrideHistory(qualifiedHistory);
            _history[qualifiedHistory.Qualifier] = qualifiedHistory;
        }

        private void Reload(IEnumerable<QualifiedValue> allValues)
        {
            if (allValues == null)
                return;

            _localizedValues = new Dictionary<Part, KeyLocaleValueContainer>();
            foreach (var qv in allValues)
            {
                var container = _localizedValues.ContainsKey(qv.Qualifier.Part)
                    ? _localizedValues[qv.Qualifier.Part]
                    : _localizedValues[qv.Qualifier.Part] = new KeyLocaleValueContainer();

                container.SetValueForKeyAndLocale(qv.Qualifier.Key, qv.Qualifier.Locale.Name, qv.Value, true);
            }
        }

        private void Reload(IEnumerable<QualifiedHistory> history)
        {
            if (history == null)
                return;

            _history = history.ToDictionary(h => h.Qualifier);
        }

        public void UpdateCreateValue(QualifiedValue value)
        {
            Reload(_connector.UpdateCreateValue(value));
        }

        public void CreateValue(Qualifier.Unique qualifier, String value)
        {
            Reload(_connector.CreateValue(qualifier, value));
        }

        public void DeleteValue(Qualifier.Unique qualifier)
        {
            Reload(_connector.DeleteValue(qualifier));
        }

        public void DeleteLocalizationsFor(Part part, String key)
        {
            Reload(_connector.DeleteLocalizationsFor(part, key));
        }

        public IEnumerable<QualifiedValue> GetAllValuesQualified()
        {
            foreach (var localization in _localizedValues)
            {
                foreach (var keyBasedLocalization in localization.Value.GetAllKeyBasedLocales())
                {
                    foreach (var languageBasedLocalization in keyBasedLocalization.Value.GetAllLocaleBasedValues())
                    {
                        var qualifier = new Qualifier.Unique(localization.Key, new Locale(languageBasedLocalization.Key), keyBasedLocalization.Key);
                        yield return new QualifiedValue(qualifier, languageBasedLocalization.Value);
                    }
                }
            }
        }

        public IEnumerable<QualifiedHistory> ProvideHistory()
        {
            return _history.Values;
        }

        public void AdjustHistory(IEnumerable<QualifiedValue> currentValues, String author)
        {
            Reload(_connector.AdjustHistory(currentValues, author));
        }

        public void RewriteHistory(IEnumerable<QualifiedHistory> newHistory)
        {
            Reload(_connector.RewriteHistory(newHistory));
        }

        public void StartBatch()
        {
            _connector.StartBatch();
        }

        public void EndBatch()
        {
            _connector.EndBatch();
            Reload(_connector.GetAllQualifiedValues());
            Reload(_connector.ProvideHistory());
        }
    }
}