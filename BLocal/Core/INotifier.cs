namespace BLocal.Core
{
    /// <summary>
    /// Responsible for handling things that go wrong with localization
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// When implemented, handles a missing localization event
        /// </summary>
        /// <param name="qualifier">Qualifier for which the value was missing</param>
        void NotifyMissing(Qualifier qualifier);
    }
}
