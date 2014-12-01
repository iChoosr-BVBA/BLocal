using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Models.ExternalSynchronization;
using BLocal.Web.Manager.Providers.ExternalSynchronizationManager;
using BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    public class ExternalSynchronizationController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }
        private readonly PartJsonConverter _partConverter = new PartJsonConverter();

        public ExternalSynchronizationController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Authenticate(ExternalSynchronizationRequest request)
        {
            var authenticationRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(request.RequestData, _partConverter);
            if (authenticationRequest.Password != ConfigurationManager.AppSettings["password"])
                return Content("", "application/json", Encoding.Unicode);

            var thisVersion = System.Reflection.Assembly.GetAssembly(typeof (HomeController)).GetName().Version.ToString();
            if(!String.Equals(authenticationRequest.Version, thisVersion))
                throw new Exception(String.Format("Trying to connect from version {0} to version {1}", authenticationRequest.Version, thisVersion));

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
            var providerGroup = GetProviderGroup(request);
            var createValueRequest = JsonConvert.DeserializeObject<CreateValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.CreateValue(createValueRequest.Qualifier, createValueRequest.Value);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult DeleteValue(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var deleteValueRequest = JsonConvert.DeserializeObject<DeleteValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.DeleteValue(deleteValueRequest.Qualifier);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult DeleteLocalizations(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var deleteLocalizationsRequest = JsonConvert.DeserializeObject<DeleteLocalizationsRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.DeleteLocalizationsFor(deleteLocalizationsRequest.Part, deleteLocalizationsRequest.Key);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult UpdateCreateValue(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var updateCreateValueRequest = JsonConvert.DeserializeObject<UpdateCreateValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.UpdateCreateValue(updateCreateValueRequest.QualifiedValue);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Reload(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var reloadRequest = JsonConvert.DeserializeObject<ReloadRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.Reload();
            var json = JsonConvert.SerializeObject(new FullContentResponse
            {
                AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray(),
                AllHistory = providerGroup.HistoryManager.ProvideHistory().ToArray()
            }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Persist(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var persistRequest = JsonConvert.DeserializeObject<PersistRequest>(request.RequestData, _partConverter);

            providerGroup.ValueManager.Persist();
            if (providerGroup.ValueManager != providerGroup.HistoryManager)
                providerGroup.HistoryManager.Persist();

            var json = JsonConvert.SerializeObject(new PersistResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult ProvideHistory(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var provideHistoryRequest = JsonConvert.DeserializeObject<ProvideHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.Reload();
            var json = JsonConvert.SerializeObject(new ProvideHistoryResponse { History = providerGroup.HistoryManager.ProvideHistory().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult AdjustHistory(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var adjustHistoryRequest = JsonConvert.DeserializeObject<AdjustHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.AdjustHistory(adjustHistoryRequest.CurrentValues, adjustHistoryRequest.Author);
            var json = JsonConvert.SerializeObject(new ProvideHistoryResponse { History = providerGroup.HistoryManager.ProvideHistory().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult OverrideHistory(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var overrideHistoryRequest = JsonConvert.DeserializeObject<OverrideHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.OverrideHistory(overrideHistoryRequest.History);
            var json = JsonConvert.SerializeObject(new OverrideHistoryResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult RewriteHistory(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var rewriteHistoryRequest = JsonConvert.DeserializeObject<RewriteHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.RewriteHistory(rewriteHistoryRequest.History);
            var json = JsonConvert.SerializeObject(new RewriteHistoryResponse { AllValues = providerGroup.HistoryManager.ProvideHistory() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult ProgressHistory(ExternalSynchronizationRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var progressHistoryRequest = JsonConvert.DeserializeObject<ProgressHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.ProgressHistory(progressHistoryRequest.Value, progressHistoryRequest.Author);
            var json = JsonConvert.SerializeObject(new ProgressHistoryResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        private ProviderGroup GetProviderGroup(ExternalSynchronizationRequest synchronizationRequest)
        {
            var dictionary = (Dictionary<Guid, SynchronizationSession>)(Request.RequestContext.HttpContext.Application["sessions"]
                ?? (Request.RequestContext.HttpContext.Application["sessions"] = new Dictionary<Guid, SynchronizationSession>()));

            if (!dictionary.ContainsKey(synchronizationRequest.ApiKey))
                throw new AuthenticationException();

            var session = dictionary[synchronizationRequest.ApiKey];

            return session.ProviderGroups.ContainsKey(synchronizationRequest.ProviderGroupName)
                ? session.ProviderGroups[synchronizationRequest.ProviderGroupName]
                : session.ProviderGroups[synchronizationRequest.ProviderGroupName] =
                    ProviderGroupFactory.CreateProviderGroup(synchronizationRequest.ProviderGroupName);
        }
    }
}