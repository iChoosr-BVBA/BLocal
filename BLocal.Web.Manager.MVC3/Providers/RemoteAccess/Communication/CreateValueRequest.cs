using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.RemoteAccess.Communication
{
    public class CreateValueRequest : IRequest<FullContentResponse>
    {
        public string Path { get { return "CreateValue"; }}

        public Qualifier.Unique Qualifier { get; set; }
        public String Value { get; set; }
    }
}