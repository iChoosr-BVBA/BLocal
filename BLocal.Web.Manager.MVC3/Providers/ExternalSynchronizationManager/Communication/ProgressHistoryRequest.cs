using System;
using BLocal.Core;

namespace BLocal.Web.Manager.Providers.ExternalSynchronizationManager.Communication
{
    public class ProgressHistoryRequest : IRequest<ProgressHistoryResponse>
    {
        public string Path { get { return "ProgressHistory"; }}

        public QualifiedValue Value { get; set; }
        public String Author { get; set; }
    }
}