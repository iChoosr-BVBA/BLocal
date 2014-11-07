using System;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class AuthenticationRequest : IRequest<AuthenticationResponse>
    {
        public String Path { get { return "Authenticate"; } }

        public String Password { get; set; }
    }
}