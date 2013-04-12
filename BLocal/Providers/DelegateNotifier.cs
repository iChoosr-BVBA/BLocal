using System;
using BLocal.Core;

namespace BLocal.Providers
{
    public class DelegateNotifier : INotifier
    {
        private readonly Action<Qualifier> _missingValueAction;

        public DelegateNotifier(Action<Qualifier> missingValueAction)
        {
            _missingValueAction = missingValueAction;
        }

        public void NotifyMissing(Qualifier qualifier)
        {
            if (_missingValueAction != null)
                _missingValueAction(qualifier);
        }
    }
}
