namespace BLocal.Core
{
    /// <summary>
    /// Provides access to the current part
    /// </summary>
    public interface IPartProvider
    {
        /// <summary>
        /// When implemented, returns the current part.
        /// </summary>
        /// <returns></returns>
        Part GetCurrentPart();
    }
}
