namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class ProvideHistoryRequest : IRequest<ProvideHistoryResponse>
    {
        public string Path { get { return "ProvideHistory"; } }
    }
}