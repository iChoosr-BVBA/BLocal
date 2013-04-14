using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Throws an exception localized value cannot be found
    /// </summary>
    public class ExceptionNotifier : INotifier
    {
        public void NotifyMissing(Qualifier qualifier)
        {
            throw new ValueNotFoundException(qualifier);
        }
    }
}
