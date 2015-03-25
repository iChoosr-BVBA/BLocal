using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess
{
    public class RemoteChainingException : Exception
    {
        public RemoteChainingException() : base("For security reasons, it is not allowed to chain RemoteAccessProviders.")
        {
        }
    }
}