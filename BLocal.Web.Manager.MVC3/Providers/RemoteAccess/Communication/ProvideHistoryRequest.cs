namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class ProvideHistoryRequest : IRequest<ProvideHistoryResponse>
    {
        public string Path { get { return "ProvideHistory"; } }
    }
}