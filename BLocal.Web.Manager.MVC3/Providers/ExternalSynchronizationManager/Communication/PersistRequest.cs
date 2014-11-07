namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class PersistRequest : IRequest<PersistResponse>
    {
        public string Path { get { return "Persist"; } }
    }
}