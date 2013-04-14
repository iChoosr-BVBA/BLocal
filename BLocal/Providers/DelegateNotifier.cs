using System;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// For when writing your own implementation of INotifier is just too much work :P
    /// </summary>
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
