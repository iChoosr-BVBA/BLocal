using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess
{
    public interface IRequest<TResponse>
    {
        String Path { get; }
    }
}