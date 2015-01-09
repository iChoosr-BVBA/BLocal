using System;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class AuthenticationRequest : IRequest<AuthenticationResponse>
    {
        public String Path { get { return "Authenticate"; } }

        public String Password { get; set; }
        public String Version { get; set; }
    }
}