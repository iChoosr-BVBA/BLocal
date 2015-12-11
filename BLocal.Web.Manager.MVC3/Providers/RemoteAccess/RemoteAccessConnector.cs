using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using BLocal.Core;
using BLocal.Web.Manager.Providers.RemoteAccess.Communication;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Providers.RemoteAccess
{
    public class RemoteAccessConnector
    {
        private Guid ApiKey { get; set; }
        private String BaseUrl { get; set; }
        private String ProviderGroupName { get; set; }

        private bool _batchMode = false;
        private List<RemoteAccessRequest> _pendingRequests = new List<RemoteAccessRequest>();
        private readonly HashSet<Type> _batchAllowedRequestTypes = new HashSet<Type>
        {
            typeof(CreateValueRequest),
            typeof(DeleteLocalizationsRequest),
            typeof(DeleteValueRequest),
            typeof(OverrideHistoryRequest),
            typeof(PersistRequest),
            typeof(ProgressHistoryRequest),
            typeof(UpdateCreateValueRequest)
        };  

        private readonly PartJsonConverter _partConverter = new PartJsonConverter();

        public RemoteAccessConnector(String baseUrl, String providerGroupName)
        {
            BaseUrl = baseUrl;
            ProviderGroupName = providerGroupName;
        }

        public void Authenticate(String password)
        {
            var request = new AuthenticationRequest
            {
                Password = password,
                Version = System.Reflection.Assembly.GetAssembly(typeof(RemoteAccessConnector)).GetName().Version.ToString()
            };

            var response = MakeRequest(request);
            ApiKey = response.ApiKey;
        }

        public IEnumerable<QualifiedValue> UpdateCreateValue(QualifiedValue value)
        {
            var request = new UpdateCreateValueRequest { QualifiedValue = value };
            var response = MakeRequest(request);
            return response == null ? null : response.AllValues;
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
            return response == null ? null : response.AllValues;
        }

        public IEnumerable<QualifiedValue> DeleteValue(Qualifier.Unique qualifier)
        {
            var request = new DeleteValueRequest { Qualifier = qualifier };
            var response = MakeRequest(request);
            return response == null ? null : response.AllValues;
        }

        public IEnumerable<QualifiedValue> DeleteLocalizationsFor(Part part, String key)
        {
            var request = new DeleteLocalizationsRequest { Part = part, Key = key };
            var response = MakeRequest(request);
            return response == null ? null : response.AllValues;
        }

        public IEnumerable<QualifiedHistory> RewriteHistory(IEnumerable<QualifiedHistory> history)
        {
            var request = new RewriteHistoryRequest { History = history.ToArray() };
            var response = MakeRequest(request);
            return response.AllValues;
        }

        public IEnumerable<QualifiedHistory> ProvideHistory()
        {
            var request = new ProvideHistoryRequest();
            var response = MakeRequest(request);
            return response.History;
        }

        public IEnumerable<QualifiedHistory> AdjustHistory(IEnumerable<QualifiedValue> currentValues, String author)
        {
            var request = new AdjustHistoryRequest { CurrentValues = currentValues.ToArray(), Author = author };
            var response = MakeRequest(request);
            return response.History;
        }

        public void OverrideHistory(QualifiedHistory qualifiedHistory)
        {
            var request = new OverrideHistoryRequest { History = qualifiedHistory };
            var response = MakeRequest(request);
        }

        public void ProgressHistory(QualifiedValue value, String author)
        {
            var request = new ProgressHistoryRequest { Value = value, Author = author };
            var response = MakeRequest(request);
        }

        public void Persist()
        {
            var request = new PersistRequest();
            var response = MakeRequest(request);
        }

        public void StartBatch()
        {
            _batchMode = true;
        }

        public void EndBatch()
        {
            _batchMode = false;
            var requests = _pendingRequests.ToList();
            _pendingRequests = new List<RemoteAccessRequest>();
            var batchRequest = new ProcessBatchRequest {Requests = requests};
            MakeRequest(batchRequest);
        }

        private TResponse MakeRequest<TResponse>(IRequest<TResponse> request) where TResponse : class
        {
            var serializedRequest = new RemoteAccessRequest
            {
                ApiKey = ApiKey,
                ProviderGroupName = ProviderGroupName,
                RequestData = JsonConvert.SerializeObject(request, _partConverter)
            };

            if (_batchMode && _batchAllowedRequestTypes.Contains(request.GetType()))
            {
                _pendingRequests.Add(serializedRequest);
                return null;
            }

            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    {"ApiKey", serializedRequest.ApiKey.ToString()},
                    {"ProviderGroupName", serializedRequest.ProviderGroupName},
                    {"RequestData", serializedRequest.RequestData}
                };

                    var response = client.UploadValues(BaseUrl + request.Path, values);
                    var responseString = Encoding.Unicode.GetString(response);
                try
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseString, _partConverter);
                }
                catch (JsonReaderException e)
                {
                    throw new Exception("Unexpected response: " + responseString, e);
                }
            }
        }
    }
}