namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class PersistRequest : IRequest<PersistResponse>
    {
        public string Path { get { return "Persist"; } }
    }
}