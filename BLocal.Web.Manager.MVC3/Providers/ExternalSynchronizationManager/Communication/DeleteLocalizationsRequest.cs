using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class DeleteLocalizationsRequest : IRequest<FullContentResponse>
    {
        public string Path { get { return "DeleteLocalizations"; } }

        public Part Part { get; set; }
        public string Key { get; set; }
    }
}