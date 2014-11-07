using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class DeleteValueRequest : IRequest<FullContentResponse>
    {
        public string Path { get { return "DeleteValue"; } }

        public Qualifier.Unique Qualifier { get; set; }
    }
}