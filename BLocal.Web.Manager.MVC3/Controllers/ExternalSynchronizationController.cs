using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Providers.ExternalSynchronizationManager;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    public class ExternalSynchronizationController : Controller
    {
        public ProviderPairFactory ProviderPairFactory { get; set; }
        private readonly PartJsonConverter _partConverter = new PartJsonConverter();

        public ExternalSynchronizationController()
        {
            ProviderPairFactory = new ProviderPairFactory();
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Authenticate(ExternalSynchronizationRequest request)
        {
            var authenticationRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(request.RequestData, _partConverter);
            if (authenticationRequest.Password != ConfigurationManager.AppSettings["password"])
                return Content("", "application/json", Encoding.Unicode);

            var dictionary = (Dictionary<Guid, SynchronizationSession>)(Request.RequestContext.HttpContext.Application["sessions"]
                ?? (Request.RequestContext.HttpContext.Application["sessions"] = new Dictionary<Guid, SynchronizationSession>()));
            
            var key = Guid.NewGuid();
            dictionary[key] = new SynchronizationSession();

            // clean up unnecesary sessions here, not perfect but it'll do
            foreach (var entry in dictionary.ToArray())
            {
                if (entry.Value.StartDateTime < DateTime.Now.AddDays(-1))
                    dictionary.Remove(entry.Key);
            }

            var json = JsonConvert.SerializeObject(new AuthenticationResponse { ApiKey = key }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult CreateValue(ExternalSynchronizationRequest request)
        {
            var providerPair = GetProviderPair(request);
            var createValueRequest = JsonConvert.DeserializeObject<CreateValueRequest>(request.RequestData, _partConverter);
            providerPair.ValueManager.CreateValue(createValueRequest.Qualifier, createValueRequest.Value);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerPair.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult DeleteValue(ExternalSynchronizationRequest request)
        {
            var providerPair = GetProviderPair(request);
            var deleteValueRequest = JsonConvert.DeserializeObject<DeleteValueRequest>(request.RequestData, _partConverter);
            providerPair.ValueManager.DeleteValue(deleteValueRequest.Qualifier);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerPair.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult DeleteLocalizations(ExternalSynchronizationRequest request)
        {
            var providerPair = GetProviderPair(request);
            var deleteLocalizationsRequest = JsonConvert.DeserializeObject<DeleteLocalizationsRequest>(request.RequestData, _partConverter);
            providerPair.ValueManager.DeleteLocalizationsFor(deleteLocalizationsRequest.Part, deleteLocalizationsRequest.Key);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerPair.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult UpdateCreateValue(ExternalSynchronizationRequest request)
        {
            var providerPair = GetProviderPair(request);
            var updateCreateValueRequest = JsonConvert.DeserializeObject<UpdateCreateValueRequest>(request.RequestData, _partConverter);
            providerPair.ValueManager.UpdateCreateValue(updateCreateValueRequest.QualifiedValue);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerPair.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Reload(ExternalSynchronizationRequest request)
        {
            var providerPair = GetProviderPair(request);
            var reloadRequest = JsonConvert.DeserializeObject<ReloadRequest>(request.RequestData, _partConverter);
            providerPair.ValueManager.Reload();
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerPair.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        private ProviderPair GetProviderPair(ExternalSynchronizationRequest synchronizationRequest)
        {
            var dictionary = (Dictionary<Guid, SynchronizationSession>)(Request.RequestContext.HttpContext.Application["sessions"]
                ?? (Request.RequestContext.HttpContext.Application["sessions"] = new Dictionary<Guid, SynchronizationSession>()));

            if(!dictionary.ContainsKey(synchronizationRequest.ApiKey))
                throw new AuthenticationException();

            var session = dictionary[synchronizationRequest.ApiKey];

            return session.ProviderPairs.ContainsKey(synchronizationRequest.ProviderPairName)
                ? session.ProviderPairs[synchronizationRequest.ProviderPairName]
                : session.ProviderPairs[synchronizationRequest.ProviderPairName] =
                    ProviderPairFactory.CreateProviderPair(synchronizationRequest.ProviderPairName);
        }

        public class ExternalSynchronizationRequest
        {
            public Guid ApiKey { get; set; }
            public String ProviderPairName { get; set; }
            public String RequestData { get; set; }
        }

        public class SynchronizationSession
        {
            public DateTime StartDateTime { get; private set; }
            public readonly Dictionary<String, ProviderPair> ProviderPairs = new Dictionary<string, ProviderPair>();

            public SynchronizationSession()
            {
                StartDateTime = DateTime.Now;
            }
        }
    }
}