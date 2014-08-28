using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public interface IRequest<TResponse>
    {
        String Path { get; }
    }
}