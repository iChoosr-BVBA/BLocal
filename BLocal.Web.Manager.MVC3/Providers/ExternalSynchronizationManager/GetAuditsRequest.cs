namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class GetAuditsRequest : IRequest<GetAuditsResponse>
    {
        public string Path { get { return "GetAudits"; } }
    }
}