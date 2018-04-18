using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Web.Mvc;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Models.ExternalSynchronization;
using BLocal.Web.Manager.Providers.RemoteAccess;
using BLocal.Web.Manager.Providers.RemoteAccess.Communication;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    public class RemoteAccessController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }
        private readonly PartJsonConverter _partConverter = new PartJsonConverter();

        public RemoteAccessController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        private static readonly Dictionary<string, MethodInfo> RequestMethods = typeof(RemoteAccessController)
            .GetMethods()
            .Where(m => m.ReturnType == typeof(ContentResult))
            .Where(m => m.GetParameters().First().ParameterType == typeof(RemoteAccessRequest))
            .Where(m => m.GetParameters().Length == 1)
            .ToDictionary(m => m.Name);

        [HttpPost, ValidateInput(false)]
        public ContentResult Authenticate(RemoteAccessRequest request)
        {
            var authenticationRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(request.RequestData, _partConverter);
            if (authenticationRequest.Password != ConfigurationManager.AppSettings["password"])
                return Content("", "application/json", Encoding.Unicode);

            var thisVersion = Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
            if (!String.Equals(authenticationRequest.Version, thisVersion))
                return Content(String.Format("Trying to connect from version {0} to version {1}", authenticationRequest.Version ?? "N/A", thisVersion), "application/text", Encoding.Unicode);

            var dictionary = (Dictionary<Guid, SynchronizationSession>)(Request.RequestContext.HttpContext.Application["sessions"]
                ?? (Request.RequestContext.HttpContext.Application["sessions"] = new Dictionary<Guid, SynchronizationSession>()));

            var key = Guid.NewGuid();
            dictionary[key] = new SynchronizationSession();

            CleanSessions(dictionary);

            var json = JsonConvert.SerializeObject(new AuthenticationResponse { ApiKey = key }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult CreateValue(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var createValueRequest = JsonConvert.DeserializeObject<CreateValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.CreateValue(createValueRequest.Qualifier, createValueRequest.Value);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult DeleteValue(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var deleteValueRequest = JsonConvert.DeserializeObject<DeleteValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.DeleteValue(deleteValueRequest.Qualifier);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult UpdateCreateValue(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var updateCreateValueRequest = JsonConvert.DeserializeObject<UpdateCreateValueRequest>(request.RequestData, _partConverter);
            providerGroup.ValueManager.UpdateCreateValue(updateCreateValueRequest.QualifiedValue);
            var json = JsonConvert.SerializeObject(new FullContentResponse { AllValues = providerGroup.ValueManager.GetAllValuesQualified().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult Reload(RemoteAccessRequest request)
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
        public ContentResult Persist(RemoteAccessRequest request)
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
        public ContentResult ProvideHistory(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var provideHistoryRequest = JsonConvert.DeserializeObject<ProvideHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.Reload();
            var json = JsonConvert.SerializeObject(new ProvideHistoryResponse { History = providerGroup.HistoryManager.ProvideHistory().ToArray() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult OverrideHistory(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var overrideHistoryRequest = JsonConvert.DeserializeObject<OverrideHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.OverrideHistory(overrideHistoryRequest.History);
            var json = JsonConvert.SerializeObject(new OverrideHistoryResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult RewriteHistory(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var rewriteHistoryRequest = JsonConvert.DeserializeObject<RewriteHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.RewriteHistory(rewriteHistoryRequest.History);
            var json = JsonConvert.SerializeObject(new RewriteHistoryResponse { AllValues = providerGroup.HistoryManager.ProvideHistory() }, _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult ProgressHistory(RemoteAccessRequest request)
        {
            var providerGroup = GetProviderGroup(request);
            var progressHistoryRequest = JsonConvert.DeserializeObject<ProgressHistoryRequest>(request.RequestData, _partConverter);
            providerGroup.HistoryManager.ProgressHistory(progressHistoryRequest.Value, progressHistoryRequest.Author);
            var json = JsonConvert.SerializeObject(new ProgressHistoryResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        [HttpPost, ValidateInput(false)]
        public ContentResult ProcessBatch(RemoteAccessRequest request)
        {
            var processBatchRequest = JsonConvert.DeserializeObject<ProcessBatchRequest>(request.RequestData);

            foreach (var batchRequest in processBatchRequest.Requests)
            {
                RequestMethods[JsonConvert.DeserializeObject<BasicRequest>(batchRequest.RequestData).Path]
                    .Invoke(this, new object[] {batchRequest});
            }
            var json = JsonConvert.SerializeObject(new ProcessBatchResponse(), _partConverter);
            return Content(json, "application/json", Encoding.Unicode);
        }

        private void CleanSessions(Dictionary<Guid, SynchronizationSession> dictionary)
        {
            // clean up unnecesary sessions here, not perfect but it'll do
            foreach (var entry in dictionary.ToArray())
            {
                if (entry.Value.StartDateTime < DateTime.Now.AddMinutes(-5))
                    dictionary.Remove(entry.Key);
            }
        }

        private ProviderGroup GetProviderGroup(RemoteAccessRequest synchronizationRequest)
        {
            var dictionary = (Dictionary<Guid, SynchronizationSession>)(Request.RequestContext.HttpContext.Application["sessions"]
                ?? (Request.RequestContext.HttpContext.Application["sessions"] = new Dictionary<Guid, SynchronizationSession>()));

            CleanSessions(dictionary);

            if (!dictionary.ContainsKey(synchronizationRequest.ApiKey))
                throw new AuthenticationException();

            var session = dictionary[synchronizationRequest.ApiKey];

            var group = session.ProviderGroups.ContainsKey(synchronizationRequest.ProviderGroupName)
                ? session.ProviderGroups[synchronizationRequest.ProviderGroupName]
                : session.ProviderGroups[synchronizationRequest.ProviderGroupName] =
                    ProviderGroupFactory.CreateProviderGroup(synchronizationRequest.ProviderGroupName);

            var isChainingRemote = group.HistoryManager.GetType() == typeof(RemoteAccessManager) || group.ValueManager.GetType() == typeof(RemoteAccessManager);
            if(isChainingRemote)
                throw new RemoteChainingException();

            return group;
        }
    }
}