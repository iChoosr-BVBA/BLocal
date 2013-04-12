using BLocal.Core;

namespace BLocal.Providers
{
    public class ExceptionNotifier : INotifier
    {
        public ExceptionNotifier()
        {
        }

        public void NotifyMissing(Qualifier qualifier)
        {
            throw new ValueNotFoundException(qualifier);
        }
    }
}
