using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager
{
    public class AdjustHistoryRequest : IRequest<AdjustHistoryResponse>
    {
        public string Path { get { return "AdjustHistory"; } }
        public QualifiedValue[] CurrentValues { get; set; }
        public String Author { get; set; }
    }
}