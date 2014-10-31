namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class PersistRequest : IRequest<PersistResponse>
    {
        public string Path { get { return "Persist"; } }
    }
}