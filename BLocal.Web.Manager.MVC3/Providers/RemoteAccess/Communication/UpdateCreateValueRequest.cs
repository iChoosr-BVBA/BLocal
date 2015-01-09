using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class UpdateCreateValueRequest: IRequest<FullContentResponse>
    {
        public String Path { get { return "UpdateCreateValue"; } }

        public QualifiedValue QualifiedValue { get; set; }
    }
}