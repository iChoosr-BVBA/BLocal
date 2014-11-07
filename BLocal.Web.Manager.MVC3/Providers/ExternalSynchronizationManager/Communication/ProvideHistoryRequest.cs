namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ProvideHistoryRequest : IRequest<ProvideHistoryResponse>
    {
        public string Path { get { return "ProvideHistory"; } }
    }
}