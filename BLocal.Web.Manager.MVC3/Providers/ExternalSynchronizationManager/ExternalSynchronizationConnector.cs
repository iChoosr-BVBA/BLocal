using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using BLocal.Core;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class ExternalSynchronizationConnector
    {
        private Guid ApiKey { get; set; }
        private String BaseUrl { get; set; }
        private String ProviderPairName { get; set; }

        private readonly PartJsonConverter _partConverter = new PartJsonConverter();

        public ExternalSynchronizationConnector(String baseUrl, String providerPairName)
        {
            BaseUrl = baseUrl;
            ProviderPairName = providerPairName;
        }

        public void Authenticate(String password)
        {
            var request = new AuthenticationRequest {Password = password};
            var response = MakeRequest(request);
            ApiKey = response.ApiKey;
        }

        public IEnumerable<QualifiedValue> UpdateCreateValue(QualifiedValue value)
        {
            var request = new UpdateCreateValueRequest { QualifiedValue = value };
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public IEnumerable<QualifiedValue> GetAllQualifiedValues()
        {
            var request = new ReloadRequest();
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public IEnumerable<QualifiedValue> CreateValue(Qualifier.Unique qualifier, String value)
        {
            var request = new CreateValueRequest { Qualifier = qualifier, Value = value };
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public IEnumerable<QualifiedValue> DeleteValue(Qualifier.Unique qualifier)
        {
            var request = new DeleteValueRequest { Qualifier = qualifier };
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public IEnumerable<QualifiedValue> DeleteLocalizationsFor(Part part, String key)
        {
            var request = new DeleteLocalizationsRequest { Part = part, Key = key };
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public void SetAudits(IEnumerable<LocalizationAudit> audits)
        {
            var request = new SetAuditsRequest { Audits = audits.ToArray() };
            MakeRequest(request);
        }

        public IEnumerable<LocalizationAudit> GetAudits()
        {
            var request = new GetAuditsRequest();
            var response = MakeRequest(request);
            return response.Audits;
        }

        public void Persist()
        {
            var request = new PersistRequest();
            var response = MakeRequest(request);
        }

        private TResponse MakeRequest<TResponse>(IRequest<TResponse> request)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    {"ApiKey", ApiKey.ToString()},
                    {"ProviderPairName", ProviderPairName},
                    {"RequestData", JsonConvert.SerializeObject(request, _partConverter)}
                };

                var response = client.UploadValues(BaseUrl + request.Path, values);
                var responseString = Encoding.Unicode.GetString(response);
                return JsonConvert.DeserializeObject<TResponse>(responseString, _partConverter);
            }
        }
    }
}