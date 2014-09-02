using System;
using System.Collections.Generic;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class ExternalSynchronizationManager : ILocalizedValueManager
    {
        private readonly string _targetPassword;
        private readonly ExternalSynchronizationConnector _connector;
        private Dictionary<Part, KeyLocaleValueContainer> _localizedValues = new Dictionary<Part, KeyLocaleValueContainer>(); 

        public ExternalSynchronizationManager(String targetUrl, String targetPassword, String targetProviderPair)
        {
            _targetPassword = targetPassword;
            _connector = new ExternalSynchronizationConnector(targetUrl, targetProviderPair);
        }

        public String GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            return GetQualifiedValue(qualifier, defaultValue).Value;
        }

        public void SetValue(Qualifier.Unique qualifier, String value)
        {
            _connector.UpdateCreateValue(new QualifiedValue(qualifier, value));
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

        public void Reload()
        {
            _connector.Authenticate(_targetPassword);
            Reload(_connector.GetAllQualifiedValues());
        }

        private void Reload(IEnumerable<QualifiedValue> allValues)
        {
            _localizedValues = new Dictionary<Part, KeyLocaleValueContainer>();
            foreach (var qv in allValues)
            {
                var container = _localizedValues.ContainsKey(qv.Qualifier.Part)
                    ? _localizedValues[qv.Qualifier.Part]
                    : _localizedValues[qv.Qualifier.Part] = new KeyLocaleValueContainer();

                container.SetValueForKeyAndLocale(qv.Qualifier.Key, qv.Qualifier.Locale.Name, qv.Value, true);
            }
        }

        public void UpdateCreateValue(QualifiedValue value)
        {
            Reload(_connector.UpdateCreateValue(value));
        }

        public void CreateValue(Qualifier.Unique qualifier, String value)
        {
            Reload(_connector.CreateValue(qualifier, value));
        }

        public void SetAudits(IEnumerable<LocalizationAudit> audits)
        {
            _connector.SetAudits(audits);
        }

        public void DeleteValue(Qualifier.Unique qualifier)
        {
            Reload(_connector.DeleteValue(qualifier));
        }

        public void DeleteLocalizationsFor(Part part, String key)
        {
            Reload(_connector.DeleteLocalizationsFor(part, key));
        }

        public IEnumerable<LocalizationAudit> GetAudits()
        {
            return _connector.GetAudits();
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
    }
}